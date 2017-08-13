using System;
using Client.ServiceReference1;
namespace Client
{
        class Program
    {
        static void Main()
        {
            while(true)
            {
                string Location_Begin = "";
                string Location_End = "";
                // Entrer l'adresse de départ
                Console.WriteLine("Entrez l'adresse de départ : ");
                Location_Begin = Console.ReadLine();
                //Location_Begin = "121 Avenue Foch, 94210 Saint-Maur-des-Fossés";

                // Entrer l'adresse de fin 
                Console.WriteLine("Entrez l'adresse de fin : ");
                Location_End = Console.ReadLine();
                //Location_End = "1 Avenue Jean Jaurès, 92150 Suresnes";


                TrajetServiceClient service = new TrajetServiceClient();
                String response = service.submit(Location_Begin, Location_End);
                Console.WriteLine(response);

            }
        }
    
    }
}

