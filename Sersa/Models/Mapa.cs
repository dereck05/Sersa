﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Kernel.Geom;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using iText.Layout.Element;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Properties;
using Rectangle = iText.Kernel.Geom.Rectangle;
using iText.IO.Image;
using System.Net;

namespace Sersa.Models
{
    public class Mapa
    {
        internal DBConnector Database { get; set; }
        public string latitud { get; set; }
        public string longitud { get; set; }
        public int tipo { get; set; }

        public Mapa() { }
        public Mapa(string pLatitud, string pLongitud, int pTipo)
        {
            latitud = pLatitud;
            longitud = pLongitud;
            tipo = pTipo;
        }

        public IConfigurationRoot GetConnection()
        {

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appSettings.json").Build();

            return builder;

        }

        internal Mapa(DBConnector db)
        {
            Database = db;
        }

        public List<Mapa> obtenerPuntos(long fechaInicio, long fechaFin, List<int> tipos) {

            string asada = Autenticacion.get_idAsada();

            string res = "";

            //Conexion escondida
            var connection = GetConnection().GetSection("ConnectionStrings").GetSection("Sersa").Value;
            MySqlConnection conn = new MySqlConnection(connection);
            conn.Open();

            res += "(";

            foreach (int tipo in tipos) {
                //res += "'";
                res += tipo.ToString();
                res += ",";
            }
            res.TrimEnd(',');
            res += ")";

            string sql1 = "SELECT f.tipo_formulario, f.latitud, f.longitud FROM sersa.Formulario AS f WHERE (f.fecha BETWEEN @fechaI AND @fechaF) AND (f.tipo_formulario IN";
            sql1 += "(";
            foreach (int tipo in tipos) {
                sql1 += "'" + tipo.ToString() + "',";
            }
            string sql = sql1.Remove(sql1.Length - 1, 1);
            sql += ")) AND (f.asada = @asada)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fechaI", fechaInicio);
            cmd.Parameters.AddWithValue("@fechaF", fechaFin);
            cmd.Parameters.AddWithValue("@asada", asada);
            cmd.Parameters.AddWithValue("@res", res);
            MySqlDataReader rdr = cmd.ExecuteReader();

            List<Mapa> lista = new List<Mapa>();

            while (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Mapa temp = new Mapa();
                    int col1Value = (int)rdr[0];
                    temp.tipo = col1Value;
                    string col2Value = rdr[1].ToString();
                    temp.latitud = col2Value;
                    string col3Value = rdr[2].ToString();
                    temp.longitud = col3Value;
                    lista.Add(temp);
                }
                rdr.NextResult();
            }
            return lista;
        }

    }
}
