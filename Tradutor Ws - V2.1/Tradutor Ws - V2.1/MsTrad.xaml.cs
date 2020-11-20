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
        public string Texto { get; set; }
        public string To { get; set; }
        public string Language { get; set; }
    }

    public class ListaId
    {
        public string Cod { get; set; }
        public string Nome { get; set; }
    }

    public partial class MsTrad : Window
    {
        private readonly string subscriptionKey = "b38bee5190mshb800f582c906e34p1004efjsn1d9fef204f63";
        private readonly string endpoint = "https://microsoft-translator-text.p.rapidapi.com/";
        private readonly string location = "AWS - ap-southeast-1";
        private Dictionary<string, string> DicIdioma = new Dictionary<string, string>();
        private List<ListaId> lista = new List<ListaId>();
        
        public MsTrad()
        {
            InitializeComponent();
            PreencheCombo();
        }

        //Preenche os combo box com os idiomas
        private void PreencheCombo()
        {
            DicIdioma.Clear();
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(endpoint + "languages?api-version=3.0");
            request.Headers.Add("accept-language", "pt");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-host", "microsoft-translator-text.p.rapidapi.com");
            HttpResponseMessage thing = client.SendAsync(request).Result;
            var actualResponse = thing.Content.ReadAsStringAsync().Result;
            TextSaidaIdioma.Text = JsonConvert.DeserializeObject(actualResponse).Totring().Replace("\"", "'").toString();
            var json = @"" + TextSaidaIdioma.Text + "";
            
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
                            DicIdioma.Add(id, reader.Value.ToString());
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
                    var idiomas = DicIdioma.Values.ToList();
                    idiomas.Sort();
                    foreach (var idioma in idiomas)
                    {
                        CboFrom.Items.Add(idioma);
                        CboTo.Items.Add(idioma);
                    }
                }
            }
        }

        //Traduz o texto inserido no campo de entrada
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Ie = TextIdiomaDetect.Text; //Idioma de entrada
            string Is = TextIdSaida.Text; //Idioma de saida
            string textToTranslate = TextFrom.Text;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + "translate?to="+Is+"&api-version=3.0&from="+Ie+"&profanityAction=NoAction&textType=plain");
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-Region", location);
            HttpResponseMessage thing = client.SendAsync(request).Result;
            string actualResponse = thing.Content.ReadAsStringAsync().Result;
            string json = @"{" + JsonConvert.DeserializeObject(actualResponse).ToString().Remove(0, 48).Remove(S.Length - 23).Replace("\"", "'").ToString() + "}";
            TradutorSaida tradu = JsonConvert.DeserializeObject<TradutorSaida>(json);
            TextTo.Text = tradu.Texto;
        }

        //Detecta o idioma inserido
       private void DetectaIdioma()
        {
            string textToTranslate = TextFrom.Text;
            var body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + "Detect?api-version=3.0");
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("x-rapidapi-key", subscriptionKey);
            request.Headers.Add("x-rapidapi-Region", location);
            HttpResponseMessage thing = client.SendAsync(request).Result;
            string actualResponse = thing.Content.ReadAsStringAsync().Result;
            string json = @"{" + JsonConvert.DeserializeObject(actualResponse).ToString().Remove(0, 10).Remove(S.Length - 7).Replace("\"", "'").ToString() + "}";
            TradutorSaida tradu = JsonConvert.DeserializeObject<TradutorSaida>(json);
            TextIdiomaDetect.Text = tradu.Language;
            SelecionaIdioma();
        }

        //Seleciona o Idioma detectado
        void SelecionaIdioma()
        {           
            string json = @"" + TextSaidaIdioma.Text + "";
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
                    foreach (var item in lista)
                    {
                        if (TextIdiomaDetect.Text == item.Cod)
                        {
                            LblIdiFrom.Content = "Traduzindo do idioma: " + item.Nome;
                            CboFrom.SelectedValue = item.Nome;
                        }
                    }
                }
            }
        }

        private void TextFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {      
            if (TextFrom.Text.Length.Equals(10)) { DetectaIdioma(); }          
        }

        //Seleciona o idioma de saida no combo
        private void CboTo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string json = @"" + TextSaidaIdioma.Text + "";
            lista.Clear();
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
                    foreach (var item in lista)
                    {
                        if (CboTo.SelectedValue.ToString() == item.Nome)
                        {
                            TextIdSaida.Text = item.Cod;
                        }
                    }
                }
            }
        }

        // Seleciona o idioma de entrada no combo
        private void CboFrom_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var json = @"" + TextSaidaIdioma.Text + "";
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
                    foreach (var item in lista)
                    {
                        if (CboFrom.SelectedValue.ToString() == item.Nome)
                        {
                            TextIdiomaDetect.Text = item.Cod;
                            LblIdiFrom.Content = "Traduzindo do idioma: " + item.Nome;
                        }
                    }
                }
            }
        }
    }
}
