using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace WebService
{
    public class Service1 : TrajetService
    {

        private XmlDocument requette(string str)
        {
            // Créer la requet URL pour récupère ses informations
            WebRequest request_for_data = WebRequest.Create(str);

            // Récupère la réponse pour ses infos
            WebResponse response_for_data = request_for_data.GetResponse();
            Stream dataStream_for_data = response_for_data.GetResponseStream();
            StreamReader reader_for_data = new StreamReader(dataStream_for_data); // Read the content.
            string responseFromServer_for_data = reader_for_data.ReadToEnd(); // Put it in a String

            // Récupère le nombre de place libre et occupée
            XmlDocument doc_for_data = new XmlDocument();
            doc_for_data.LoadXml(responseFromServer_for_data);
            reader_for_data.Close();
            response_for_data.Close();
            return doc_for_data;
        }

        private string instructions(XmlDocument doc)
        {
            string str = "";
            XmlNodeList elemList = doc.SelectNodes("/DirectionsResponse/route/leg/step");
            str = str + "Trajet en " + elemList.Count + " étapes !" + "\n";
            foreach (XmlElement elem in elemList)
            {
                str = str + "- " + elem["html_instructions"].InnerText.Replace("<b>", "").Replace("</b>", "") + " sur " + elem["distance"]["text"].InnerText + "\n";
            }
            return str;
        }

        private int duration(XmlDocument doc)
        {
            XmlNodeList elemList = doc.SelectNodes("/DirectionsResponse/route/leg");
            return Int32.Parse(elemList[0]["duration"]["value"].InnerText);
        }

        public string submit(string Location_Begin, string Location_End)
        {
            {
                string trajet = "";
                // Créer la requet URL pour le trajet Pieton de A à B
                XmlDocument doc = requette("https://maps.googleapis.com/maps/api/directions/xml?origin=" + Location_Begin + "&destination=" + Location_End + "&mode=walking&key=AIzaSyBWOayAMh6TKPXkcEvcwDQI1iKyYl0_8Ow");

                XmlNodeList elemList = doc.SelectNodes("/DirectionsResponse/route/leg");
                // On stock les coordonnées
                double Start_lat = Double.Parse(elemList[0]["start_location"]["lat"].InnerText.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                double Start_lng = Double.Parse(elemList[0]["start_location"]["lng"].InnerText.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                double End_lat = Double.Parse(elemList[0]["end_location"]["lat"].InnerText.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                double End_lng = Double.Parse(elemList[0]["end_location"]["lng"].InnerText.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                string Start_lat_s = elemList[0]["start_location"]["lat"].InnerText;
                string Start_lng_s = elemList[0]["start_location"]["lng"].InnerText;
                string End_lat_s = elemList[0]["end_location"]["lat"].InnerText;
                string End_lng_s = elemList[0]["end_location"]["lng"].InnerText;

                XmlDocument docStation = requette("http://www.velib.paris/service/carto");
                XmlNodeList elemListStation = docStation.GetElementsByTagName("marker");

                Dictionary<double, int> mapA = new Dictionary<double, int>();
                Dictionary<double, int> mapB = new Dictionary<double, int>();
                for (int i = 0; i < elemListStation.Count; i++)
                {
                    double station_lat = Double.Parse(elemListStation[i].Attributes["lat"].Value.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                    double station_lng = Double.Parse(elemListStation[i].Attributes["lng"].Value.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                    mapA.Add(Math.Sqrt(Math.Pow((Start_lat - station_lat), 2) + Math.Pow((Start_lng - station_lng), 2)), i);
                    mapB.Add(Math.Sqrt(Math.Pow((End_lat - station_lat), 2) + Math.Pow((End_lng - station_lng), 2)), i);
                }

                int StationA = -1;
                int StationB = -1;
                foreach (var item in mapA.OrderBy(i => i.Key))
                {
                    // On récûpère son numéro
                    String numPoint = elemListStation[item.Value].Attributes["number"].Value;

                    // Créer la requet URL pour récupère ses informations
                    XmlDocument doc_for_data = requette("http://www.velib.paris/service/stationdetails/" + numPoint);

                    XmlNodeList elemList_for_available = doc_for_data.GetElementsByTagName("available");

                    int available = Int32.Parse(elemList_for_available[0].FirstChild.Value);

                    if (available > 0)
                    {
                        StationA = item.Value;
                        break;
                    }

                }
                foreach (var item in mapB.OrderBy(i => i.Key))
                {
                    // On récûpère son numéro
                    String numPoint = elemListStation[item.Value].Attributes["number"].Value;

                    // Créer la requet URL pour récupère ses informations
                    XmlDocument doc_for_data = requette("http://www.velib.paris/service/stationdetails/" + numPoint);

                    XmlNodeList elemList_for_free = doc_for_data.GetElementsByTagName("free");

                    int free = Int32.Parse(elemList_for_free[0].FirstChild.Value);

                    if (free > 0)
                    {
                        StationB = item.Value;
                        break;
                    }

                }

                string stationA_lat = elemListStation[StationA].Attributes["lat"].Value;
                string stationA_lng = elemListStation[StationA].Attributes["lng"].Value;
                string stationB_lat = elemListStation[StationB].Attributes["lat"].Value;
                string stationB_lng = elemListStation[StationB].Attributes["lng"].Value;

                // Créer la requet URL pour récupère les trajets
                XmlDocument docTrajetA = requette("https://maps.googleapis.com/maps/api/directions/xml?origin=" + Start_lat_s + "%2C" + Start_lng_s + "&destination=" + stationA_lat + "%2C" + stationA_lng + "&mode=walking&key=AIzaSyBWOayAMh6TKPXkcEvcwDQI1iKyYl0_8Ow");
                XmlDocument docTrajetB = requette("https://maps.googleapis.com/maps/api/directions/xml?origin=" + stationB_lat + "%2C" + stationB_lng + "&destination=" + End_lat_s + "%2C" + End_lng_s + "&mode=walking&key=AIzaSyBWOayAMh6TKPXkcEvcwDQI1iKyYl0_8Ow");
                XmlDocument docTrajetBicycle = requette("https://maps.googleapis.com/maps/api/directions/xml?origin=" + stationA_lat + "%2C" + stationA_lng + "&destination=" + stationB_lat + "%2C" + stationB_lng + "&mode=bicycling&key=AIzaSyBWOayAMh6TKPXkcEvcwDQI1iKyYl0_8Ow");
                // Si c'est plus court à pied
                if (duration(docTrajetA) + duration(docTrajetB) + duration(docTrajetBicycle) >= duration(doc))
                {
                    trajet = trajet + "Trajet en " + (duration(doc) / 3600) % 24 + " heures " + (duration(doc) / 60) % 60 + " minutes " + duration(doc) % 60 + " s" + "\n";
                    trajet = trajet + "\nPour aller à votre destination, il vaut mieux tout faire à pied, suivez les instructions suivantes : " + "\n";
                    trajet = trajet + instructions(doc);
                    trajet = trajet + "\n";
                    trajet = trajet + "\nSinon aller à la station " + elemListStation[StationA].Attributes["name"].Value + " de départ, suivez les instructions suivantes : " + "\n";
                }
                // Sinon on prends le vélo
                else
                {
                    trajet = trajet + "Trajet en " + ((duration(docTrajetA) + duration(docTrajetB) + duration(docTrajetBicycle)) / 3600) % 24 + " heures " + ((duration(docTrajetA) + duration(docTrajetB) + duration(docTrajetBicycle)) / 60) % 60 + " minutes " + (duration(docTrajetA) + duration(docTrajetB) + duration(docTrajetBicycle)) % 60 + " s" + "\n";
                    trajet = trajet + "\nPour aller à la station " + elemListStation[StationA].Attributes["name"].Value + " de départ, suivez les instructions suivantes : " + "\n";
                }
                trajet = trajet + instructions(docTrajetA);
                trajet = trajet + "\n";
                trajet = trajet + "\nPour aller à la station " + elemListStation[StationB].Attributes["name"].Value + ", suivez les instructions suivantes : " + "\n";
                trajet = trajet + instructions(docTrajetBicycle);
                trajet = trajet + "\n";
                trajet = trajet + "\nPour aller à votre destination " + Location_End + ", suivez les instructions suivantes : " + "\n";
                trajet = trajet + instructions(docTrajetB);
                trajet = trajet + "\n";

                trajet = trajet + "Vous êtes arrivée à destination !" + "\n";

                return trajet;

            }

        }
    }
}
