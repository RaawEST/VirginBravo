using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace VirginBravo
{
    public class AppData
    {
        private static string filePath = "appData.json";
        public int LastReceiptId { get; set; }
        public string FastProduct1 { get; set; }
        public string FastProduct2 { get; set; }
        public string FastProduct3 { get; set; }
        public string FastProduct4 { get; set; }
        public string FastProduct5 { get; set; }
        public string KGBPass { get; set; }
        public ObservableCollection<Product> Products { get; set; }

        public static void SaveAppData(AppData data)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, jsonString);
        }

        public AppData GetAppData()
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);

                AppData data = JsonConvert.DeserializeObject<AppData>(jsonString);
                return data;
            }
            else
            {
               AppData newData = new AppData
                {
                    LastReceiptId = 0,

                    FastProduct1 = "Lady Drink",
                    FastProduct2 = "Viski",
                    FastProduct3 = "Tuborg",
                    FastProduct4 = "Saku Orig",
                    FastProduct5 = "Gin Toonik",

                    KGBPass = "1",

                    Products = new ObservableCollection<Product>() {
                //private
                    new Product("Privaat 10 min", "privaat", 50),
                    new Product("Privaat 20 min", "privaat", 90),
                    new Product("Privaat 30 min", "privaat", 130),
                    new Product("Privaat 40 min", "privaat", 170),
                    new Product("Privaat 50 min", "privaat", 210),
                    new Product("Privaat 60 min", "privaat", 250),
                    new Product("Priv 2m 10 min", "privaat", 140),
                    new Product("Priv 2m 20 min", "privaat", 220),
                    new Product("Priv 2m 30 min", "privaat", 300),
                    new Product("Priv 2m 40 min", "privaat", 380),
                    new Product("Priv 2m 50 min", "privaat", 460),
                    new Product("Priv 2m 60 min", "privaat", 540),
                    new Product("Sauna", "privaat", 350),
                    new Product("Sauna 2m", "privaat", 740),
                    //olu
                    new Product("Saku Orig", "beer", 7),
                    new Product("Gin Long Drink", "beer", 7),
                    new Product("Tuborg", "beer", 6),
                    new Product("Blanc", "beer", 6),
                    new Product("Somersby", "beer", 6),
                    //kokteil
                    new Product("Viski", "cocktail", 10),
                    new Product("Gin Toonik", "cocktail", 10),
                    new Product("Energia Viin", "cocktail", 10),
                    new Product("Viin Mahlaga", "cocktail", 10),
                    new Product("RummCola", "cocktail", 10),
                    new Product("ViskiCola", "cocktail", 10),
                    new Product("Vein Punane", "cocktail", 10),
                    new Product("Vein Valge", "cocktail", 10),
                    //konjak
                    new Product("Hennesy", "cognac", 15),
                    new Product("Larsen", "cognac", 15),
                    new Product("Courvoiseir", "cognac", 15),
                    //shotid
                    new Product("JagerMeister", "shot", 5),
                    new Product("Tequila", "shot", 5),
                    new Product("Viin", "shot", 5),
                    new Product("Sambuca", "shot", 5),
                    new Product("B52", "shot", 5),
                    //alkovaba
                    new Product("Carlsberg", "alcofree", 5),
                    new Product("Go Pale Ale", "alcofree", 5),
                    new Product("Red Bull", "alcofree", 5),
                    new Product("CocaCola", "alcofree", 5),
                    new Product("CocaColaZ", "alcofree", 5),
                    new Product("Kohv", "alcofree", 5),
                    new Product("Tee", "alcofree", 5),
                    new Product("Mahl", "alcofree", 5),
                    new Product("Vesi", "alcofree", 5),
                    //tudrukule
                    new Product("Lady Drink", "gift", 25),
                    new Product("Prossecco", "gift", 150),
                    new Product("Punane viin", "gift", 150),
                    new Product("Valge viin", "gift", 150),
                    new Product("M&C Brut Imp.", "gift", 250),
                    new Product("Veuve Clicquot", "gift", 300),
                    //Special Drinks
                    new Product("Fire Sambuca", "special", 25),
                    new Product("Lamborghini", "special", 25),
                    //Other
                    new Product("Shower Show", "other", 150),
                    new Product("Body Shot", "other", 20),
                    new Product("Lava Tants", "other", 65),
                    new Product("Topless seltskond", "other", 200),
                    new Product("Paljas seltskond", "other", 700),
                    new Product("Kasutatud aluspukse", "other", 215),
                    new Product("MF talks(1 k.)", "other", 500),
                    new Product("MF talks(koik)", "other", 1500),
                    new Product("Pidu parast 06:00", "other", 300),
                    //Passage, V-Bucks
                    new Product("Sissepaas", "other2", 10),
                    new Product("V-Dollar", "other2", 5)
                }

                };
                SaveAppData(newData);
                return newData;
            }
        }

    }



}
