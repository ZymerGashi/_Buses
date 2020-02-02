using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using CefSharp;
using CefSharp.WinForms;

namespace Buses
{
    public partial class Form1 : Form
    {

        String searchedPlace;
        MapUrl mapUrl;
        HttpClient client;
        RootObject list;
        public ChromiumWebBrowser myBrowser;
        StringBuilder result;
        HttpResponseMessage response;
        int index;
        
        public Form1()
        {
            InitializeComponent();

            IntitializeBrowser();

            this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.listView1.Select();
            this.listView1.HideSelection = false;
        }

        //Initializing the CefSharp browser which uses google as search engine
        public void IntitializeBrowser()
        {
            CefSettings mySettings = new CefSettings();
            Cef.Initialize(mySettings);

            myBrowser = new ChromiumWebBrowser("https://www.google.com/maps/place/Kosovo");
            panel1.Controls.Add(myBrowser);
            myBrowser.Dock = DockStyle.Fill;
            myBrowser.Update();
           // myBrowser.Load();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<Bus> busses;
            //create a HttpClient
            client = new HttpClient();
            client.BaseAddress= new Uri("https://pribus.appbit.es/api/");
            mapUrl = new MapUrl(47.5951518, -122.3316393);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //get the buses' details as JSON
            response = client.GetAsync("v1/buses/").Result;
            result = new StringBuilder(response.Content.ReadAsStringAsync().Result);
            //Deserlize the objects. Turn the JSON content to the appropriate c# objects
   
            list = JsonConvert.DeserializeObject<RootObject>(result.ToString());


            //Insert busses in the view list
            for (var i = 0; i < list.results.buses.Count; i++)
            {
                ListViewItem item = new ListViewItem(list.results.buses[i].line.name);
                listView1.Items.Insert(i, item);
            }

            mapUrl.displayMultipleMarkers(list);
            MessageBox.Show(mapUrl.completeUrlAddress);
            myBrowser.Load(mapUrl.completeUrlAddress);
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchedPlace = txtSearch.Text;
            //mapUrl = new MapUrl(searchedPlace);
            mapUrl = new MapUrl(47.5951518, -122.3316393);
            // webBrowser1.Navigate(new Uri(mapUrl.completeUrlAddress));
            myBrowser.Refresh();
        }


        //On each of the items in the view list the below event is set. Once the item is selected, based on the bus it represents it will show its position in the map
        private void ListView1_ItemActivate(object sender, EventArgs e)
        {
            getJson();
            index = listView1.SelectedItems[0].Index;
            mapUrl = new MapUrl(Double.Parse(list.results.buses[index].latitude), Double.Parse(list.results.buses[index].longitude));
            myBrowser.Load(mapUrl.completeUrlAddress);
            timer1.Start();
           
        }


        //This timer will update the current bus location shown in the map
        private void Timer1_Tick(object sender, EventArgs e)
        {
            getJson();
            mapUrl = new MapUrl(Double.Parse(list.results.buses[index].latitude), Double.Parse(list.results.buses[index].longitude));
            myBrowser.Load(mapUrl.completeUrlAddress);
        }

        //Get JSON from the specified api url
        public void getJson()
        {
            response = client.GetAsync("v1/buses/").Result;
            result = new StringBuilder(response.Content.ReadAsStringAsync().Result);
            list = JsonConvert.DeserializeObject<RootObject>(result.ToString());
            // webBrowser1.Navigate(new Uri(mapUrl.completeUrlAddress));
        }

    
    }


    class MapUrl
    {
        public const string baseUrl = @"https://www.google.com/maps"; 
        public string completeUrlAddress { get; set; }
        private double longitude { get; set; }
        private double latitude { get; set; }
        private string place { get; set; }

        //Create a map url if we want to search places on the map (currently it is not really used, I have just done it if anytime I need it)
        public MapUrl(string place)
        {
            this.completeUrlAddress = baseUrl + "/" + place+"&z=1000";
        }

        //Create map url taking in consider the longitude and latitude of the bus, taken as JSON
        public MapUrl(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            // this.completeUrlAddress = baseUrl + "/place/" + longitude+","+latitude;
            this.completeUrlAddress = baseUrl + "/?q=" + longitude+","+latitude + "&z=0";
        }


        //Displaying all the buses on the map,seems like it works for only 3 koordinates which is not what I needddddddddddd
        public void displayMultipleMarkers(RootObject list)
        {
            var allBusesCoordinates="";
            foreach (var bus in list.results.buses)
            {
                if (bus.latitude !=null)
                {
                    allBusesCoordinates += bus.longitude + "," + bus.latitude + "/";
                }
                }

            this.completeUrlAddress = baseUrl + "/dir/" +allBusesCoordinates;
        }
    }

//--------------------Classes used for deserialization---------------------------------
    public class Line
    {
        public string _id { get; set; }
        public int __v { get; set; }
        public string name { get; set; }
        public string number { get; set; }
    }

    public class Bus
    {
        public string _id { get; set; }
        public int arvento_id { get; set; }
        public Line line { get; set; }
        public string name { get; set; }
        public int __v { get; set; }
        public string status { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public double course { get; set; }
        public string speed { get; set; }
        public string street { get; set; }
        public string license_plate { get; set; }
    }

    public class Results
    {
        public List<Bus> buses { get; set; }
    }
    public class RootObject
    {
        public Results results { get; set; }
    }

}
