using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RaceGenerator.Pages
{
    public enum TimeOfDay : int
    {
        [EnumMember(Value = "Sunrise")] [Description("Sunrise")] Sunrise = 1,
        [EnumMember(Value = "Morning")] [Description("Morning")] Morning = 2,
        [EnumMember(Value = "Noon")] [Description("Noon")] Noon = 4,
        [EnumMember(Value = "Afternoon")] [Description("Afternoon")] Afternoon = 8,
        [EnumMember(Value = "Late Afternoon")] [Description("Late Afternoon")] LateAfternoon = 16,
        [EnumMember(Value = "Sunset")] [Description("Sunset")] Sunset = 32,
        [EnumMember(Value = "Night")] [Description("Night")] Night = 64
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StartType : int
    {
        [EnumMember(Value = "Standing")] Standing = 1,
        [EnumMember(Value = "Rolling")] Rolling = 2
    }

    public class Car
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("tracklist")] public string Tracklist { get; set; }

        public List<Track> Tracks { get; set; }

        [JsonProperty("timeOfDay")] public TimeOfDay TimeOfDay { get; set; }

        [JsonProperty("startType")] public StartType StartType { get; set; }

        public Car(string name, string tracklist, List<Track> tracks, TimeOfDay timeOfDay, StartType startType)
        {
            this.Name = name;
            this.Tracklist = tracklist;
            this.Tracks = tracks;
            this.TimeOfDay = timeOfDay;
            this.StartType = startType;
        }

        public void SetTracks(List<Track> tracks)
        {
            this.Tracks = tracks;
        }
    }

    public class Track
    {
        public string Name;

        public Layout[] Layouts;
    }

    public class Layout
    {
        public string Name;

        public Layout(string name)
        {
            this.Name = name;
        }
    }

    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public Car randomCar;
        public Track randomTrack;
        public Layout randomLayout;
        public StartType randomStartType;
        public string randomTimeOfDay;

        public static string ToStringEnums(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public IndexModel(IWebHostEnvironment environment)
        {
            this._environment = environment;

            List<Track> tracksA;
            List<Track> tracksB;
            List<Car> cars;

            using StreamReader r1 = new(Path.Combine(_environment.WebRootPath, "json/") + "tracks.a.json");
            tracksA = JsonConvert.DeserializeObject<List<Track>>(r1.ReadToEnd());
            tracksA.ForEach(x =>
            {
                if (x.Layouts == null || x.Layouts.Length.Equals(0)) x.Layouts = new Layout[] { new Layout("Full") };
            });

            using StreamReader r2 = new(Path.Combine(this._environment.WebRootPath, "json/") + "tracks.b.json");
            tracksB = JsonConvert.DeserializeObject<List<Track>>(r2.ReadToEnd());
            tracksB.ForEach(x =>
            {
                if (x.Layouts == null || x.Layouts.Length.Equals(0)) x.Layouts = new Layout[] { new Layout("Full") };
            });

            using StreamReader r3 = new(Path.Combine(this._environment.WebRootPath, "json/") + "cars.json");
            cars = JsonConvert.DeserializeObject<List<Car>>(r3.ReadToEnd());


            for(int i = 0; i < cars.Count; i++)
            {
                if (cars[i].Tracklist.ToLower().Equals("a")) cars[i].SetTracks(new List<Track>(tracksA));
                else if (cars[i].Tracklist.ToLower().Equals("b")) cars[i].SetTracks(new List<Track>(tracksB));
            }

            Random random = new();

            randomCar = cars[random.Next(cars.Count)];
            randomTrack = randomCar.Tracks[random.Next(randomCar.Tracks.Count)];
            randomLayout = randomTrack.Layouts[random.Next(randomTrack.Layouts.Length)];
            randomStartType = (StartType) Enum.GetValues(typeof(StartType)).GetValue(random.Next(Enum.GetValues(typeof(StartType)).Length));

            List<TimeOfDay> possibleTimeOfDays = new();
            List<TimeOfDay> allTimeOfDays = Enum.GetValues(typeof(TimeOfDay)).Cast<TimeOfDay>().ToList();

            for (int i = 0; i < allTimeOfDays.Count; i++)
            {
                if ((randomCar.TimeOfDay & allTimeOfDays[i]) == allTimeOfDays[i]) possibleTimeOfDays.Add(allTimeOfDays[i]);
            }

            randomTimeOfDay = ToStringEnums(possibleTimeOfDays[random.Next(possibleTimeOfDays.Count)]);
        }

        public void OnGet()
        {

        }
    }
}
