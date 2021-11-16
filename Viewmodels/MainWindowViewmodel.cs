using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFbase;
using ProjectCurrencyCalculator.Viewmodels;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace ProjectCurrencyCalculator.Viewmodels
{
    public class Currency
    {
        public string Name { get; set; }
        public double FromUah { get; set; }
    }
    public class Exchange
    {
        public string currency { get; set; }
        public string to_currency { get; set; }
        public string buy { get; set; }
        public string sale { get; set; }
    }
    public class MainWindowViewmodel : OnPropertyChangedHandler
    {
        private string value1;
        private string value2;
        public ObservableCollection<Exchange> exchanges = new ObservableCollection<Exchange>();
        public MainWindowViewmodel()
        {
            List<WebRequest> wbs = new List<WebRequest>()
            {
                WebRequest.Create(@"https://api.privatbank.ua/p24api/pubinfo?json&exchange&coursid=4"),
                WebRequest.Create(@"https://api.privatbank.ua/p24api/pubinfo?json&exchange&coursid=5")
            };
            foreach (var wr in wbs)
            {
                using (Stream ds = wr.GetResponse().GetResponseStream())
                {
                    Currencies.Add(new Currency { FromUah = 1.00, Name = "UAH" });
                    string json = new StreamReader(ds).ReadToEnd();
                    //Console.WriteLine(json);
                    var doc = JsonDocument.Parse(json, options: new JsonDocumentOptions() { AllowTrailingCommas = true });
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        // Console.WriteLine($"{item.GetProperty("ccy").GetString()}\n{item.GetProperty("buy").GetString()}");
                        string name = item.GetProperty("ccy").GetString();
                        Currencies.Add(new Currency()
                        {
                            Name = name,
                            FromUah = double.Parse(item.GetProperty("buy").GetString().Replace(".", ","))
                        });
                        exchanges.Add(new Exchange { currency = "UAH", to_currency = name, buy = item.GetProperty("buy").GetString(), sale = item.GetProperty("sale").GetString() });
                        OnPropertyChanged(nameof(exchanges));
                    }
                }
            }
            
            Currencies.Remove(Currencies.Last());
            Currencies.Find(o => o.Name == "BTC").FromUah *= Currencies.Find(o => o.Name == "USD").FromUah;
            //OnPropertyChanged(nameof(Currencies));
        }
        public List<string> Namesofcurrenices => Currencies.ConvertAll(o=>o.Name).Distinct().ToList();
        public List<Currency> Currencies { get; set; } = new List<Currency>();
        public ChangingItem<string> Currency1 { get; set; } = new ChangingItem<string>();
        public ChangingItem<string> Currency2 { get; set; } = new ChangingItem<string>();
        public string Default => Namesofcurrenices[0];
        public string Value1 
        { 
            get => value1;
            set
            {
                value1 = value;
                OnPropertyChanged(nameof(Value1));
                if (IndexOfCur(Currency1.Item)!=-1)
                {
                    value2 = (double.Parse(value1.Replace(".", ",")) / Currencies[IndexOfCur(Currency2.Item)].FromUah * Currencies[IndexOfCur(Currency1.Item)].FromUah).ToString();
                    OnPropertyChanged(nameof(Value2));
                }
                
            }
        }
        public int IndexOfCur(string cur)
        {
            for (int i = 0; i < Currencies.Count; i++)
            {
                if (Currencies[i].Name==cur)
                {
                    return i;
                }
            }
            return -1;
        }
        public string Value2
        {
            get => value2;
            set
            {
                value2 = value;
                OnPropertyChanged(nameof(Value1));
                if (IndexOfCur(Currency1.Item) != -1)
                {
                    value1 = (double.Parse(value2.Replace(".", ",")) / Currencies[IndexOfCur(Currency1.Item)].FromUah * Currencies[IndexOfCur(Currency2.Item)].FromUah).ToString();
                    OnPropertyChanged(nameof(Value1));
                }
            }
        }
    }
}
