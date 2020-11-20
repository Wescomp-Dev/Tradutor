using System;
using System.Windows;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // Instalar Json pelo Nuget
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Data;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace Tradutor_Ws___V2._1
{
    /// <summary>
    /// Lógica interna para MsTrad.xaml
    /// </summary>
    /// 
    public class TradutorSaida
    {
        public string text { get; set; }
        public string To { get; set; }
        public string language { get; set; }
    }

    public class ListaId
    {
        public string Cod { get; set; }
        public string Nome { get; set; }
    }

    public partial class MsTrad : Window
    {
        private static readonly string subscriptionKey = "b38bee5190mshb800f582c906e34p1004efjsn1d9fef204f63";
        private static readonly string endpoint = "https://microsoft-translator-text.p.rapidapi.com/";
        private static readonly string location = "AWS - ap-southeast-1";
        
        public MsTrad()
        {
            InitializeComponent();
            PreencheCombo();
        }

        //Preenche os combo box com os idiomas
        private void PreencheCombo()
        {
            string route = "languages?api-version=3.0";
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(endpoint + route);
            request.Headers.Add("accept-language", "pt");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-host", "microsoft-translator-text.p.rapidapi.com");
            HttpResponseMessage thing = client.SendAsync(request).Result;
            string actualResponse = thing.Content.ReadAsStringAsync().Result;
            var Saida = JsonConvert.DeserializeObject(actualResponse);
            string S = Saida.ToString();
            S = S.Replace("\"", "'");
            TextSaidaIdioma.Text = S;
            String Ss = TextSaidaIdioma.Text;
            string json = @"" + Ss + "";
            var DicIdioma = new Dictionary<string, string>();
            DicIdioma.Clear();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string id = string.Empty;
                var i = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        switch (reader.Value.ToString())
                        {
                            case "translation": break;
                            case "transliteration": Finalizar(); return;
                            case "dictionary": return;
                            case "name": i++; break;
                            case "nativeName": i++; break;
                            case "dir": break;
                            default: id = reader.Value.ToString(); break;
                        }
                    }
                    if (reader.TokenType == JsonToken.String)
                    {
                        if (i == 1)
                        {
                            i = -1;
                            DicIdioma.Add(id, reader.Value.ToString() + Environment.NewLine);
                        }
                    }
                }
            }
            void Finalizar()
            {
                if (DicIdioma.Count > 0)
                {
                    CboFrom.SelectedIndex = 0;
                    CboTo.SelectedIndex = 0;
                    var Dic = DicIdioma.Values.ToList();
                    Dic.Sort();
                    foreach (var Value in Dic)
                    {
                        CboFrom.Items.Add(Value);
                        CboTo.Items.Add(Value);
                    }
                }
            }
        }

        //Traduz o texto inserido no campo de entrada
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String Ie = TextIdiomaDetect.Text; //Idioma de entrada
            string Is = TextIdSaida.Text; //Idioma de saida
            string route = "translate?to="+Is+"&api-version=3.0&from="+Ie+"&profanityAction=NoAction&textType=plain";    
            string textToTranslate = TextFrom.Text;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-Region", location);
            HttpResponseMessage thing = client.SendAsync(request).Result;
            string actualResponse = thing.Content.ReadAsStringAsync().Result;
            var Saida = JsonConvert.DeserializeObject(actualResponse);
            string S = Saida.ToString();
            S = S.Remove(0, 48);
            S = S.Remove(S.Length - 23);
            S = S.Replace("\"", "'");
            string json = @"{" + S + "}";
            TradutorSaida tradu = JsonConvert.DeserializeObject<TradutorSaida>(json);
            string name = tradu.text;
            TextTo.Text = name;
        }

        //Detecta o idioma inserido
       private void DetectaIdioma()
        {
            string route = "Detect?api-version=3.0";
            string textToTranslate = TextFrom.Text;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-Region", location);
            HttpResponseMessage thing = client.SendAsync(request).Result;
            string actualResponse = thing.Content.ReadAsStringAsync().Result;
            var Saida = JsonConvert.DeserializeObject(actualResponse);
            string S = Saida.ToString();
            S = S.Remove(0, 10);
            S = S.Remove(S.Length - 7);
            S = S.Replace("\"", "'");
            string json = @"{" + S + "}";
            TradutorSaida tradu = JsonConvert.DeserializeObject<TradutorSaida>(json);
            string name = tradu.language;
            TextIdiomaDetect.Text = name;
            SelecionaIdioma();
        }

        //Seleciona o Idioma detectado
        void SelecionaIdioma()
        {           
            String Ss = TextSaidaIdioma.Text;
            string json = @"" + Ss + "";
            var lista = new List<ListaId>();
            lista.Clear();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string id = string.Empty;
                var i = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        switch (reader.Value.ToString())
                        {
                            case "translation": break;
                            case "transliteration": Finalizar(); return;
                            case "dictionary": return;
                            case "name": i++; break;
                            case "nativeName": i++; break;
                            case "dir": break;
                            default: id = reader.Value.ToString(); break;
                        }
                    }
                    if (reader.TokenType == JsonToken.String)
                    {
                        if (i == 1)
                        {
                            i = -1;
                            lista.Add(new ListaId()
                            {
                                Cod = id,
                                Nome = reader.Value.ToString()
                            });
                        }
                    }
                }
            }
            void Finalizar()
            {
                if (lista.Count > 0)
                {
                    foreach (var It in lista)
                    {
                        if (TextIdiomaDetect.Text == It.Cod)
                        {
                            LblIdiFrom.Content = "Traduzindo do idioma: " + It.Nome;
                            CboFrom.SelectedValue = It.Nome;
                        }
                    }
                }
            }
        }

        private void TextFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {      
            if (TextFrom.Text.Length.Equals(10))
            {
                DetectaIdioma();              
            }          
        }

        //Seleciona o idioma de saida no combo
        private void CboTo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string Ss = TextSaidaIdioma.Text;
            string json = @"" + Ss + "";
            var lista = new List<ListaId>(); lista.Clear();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string id = string.Empty;
                string nome = string.Empty;
                var i = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        switch (reader.Value.ToString())
                        {
                            case "translation": break;
                            case "transliteration": Finalizar(); return;
                            case "dictionary": return;
                            case "name": i++; break;
                            case "nativeName": i++; break;
                            case "dir": break;
                            default: id = reader.Value.ToString(); break;
                        }
                    }
                    if (reader.TokenType == JsonToken.String)
                    {
                        if (i == 1)
                        {
                            i = -1;
                            lista.Add(new ListaId()
                            {
                                Cod = id,
                                Nome = reader.Value.ToString()
                            });
                        }
                    }
                }
            }
            void Finalizar()
            {
                if (lista.Count > 0)
                {
                    foreach (var It in lista)
                    {
                        if (CboTo.SelectedValue.ToString() == It.Nome)
                        {
                            TextIdSaida.Text = It.Cod;
                        }
                    }
                }
            }
        }

        // Seleciona o idioma de entrada no combo
        private void CboFrom_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string Ss = TextSaidaIdioma.Text;
            string json = @"" + Ss + "";
            var lista = new List<ListaId>();
            lista.Clear();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                string id = string.Empty;
                var i = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        switch (reader.Value.ToString())
                        {
                            case "translation": break;
                            case "transliteration": Finalizar(); return;
                            case "dictionary": return;
                            case "name": i++; break;
                            case "nativeName": i++; break;
                            case "dir": break;
                            default: id = reader.Value.ToString(); break;
                        }
                    }
                    if (reader.TokenType == JsonToken.String)
                    {
                        if (i == 1)
                        {
                            i = -1;
                            lista.Add(new ListaId()
                            {
                                Cod = id,
                                Nome = reader.Value.ToString()
                            });
                        }
                    }
                }
            }
            void Finalizar()
            {
                if (lista.Count > 0)
                {
                    foreach (var It in lista)
                    {
                        if (CboFrom.SelectedValue.ToString() == It.Nome)
                        {
                            TextIdiomaDetect.Text = It.Cod;
                            LblIdiFrom.Content = "Traduzindo do idioma: " + It.Nome;
                        }
                    }
                }
            }
        }
    }
}
