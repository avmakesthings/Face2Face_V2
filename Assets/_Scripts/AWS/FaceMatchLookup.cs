using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FaceData {
    

    public class PersonData
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("imagePath")]
        public string profileImagePath { get; set; }

        [JsonProperty("profileURL")]
        public string profileURL { get; set; }

        public bool attendingEvent { get; set; }

        public bool publicProfile { get; set; }

        public AdditionalPublicPersonData additionalPublicPersonData;

    }


    public class AdditionalPublicPersonData{
        [JsonProperty("bio")]
        public string[] bio { get; set; }

        [JsonProperty("education")]
        public string[] education { get; set; }

        [JsonProperty("imagePaths")]
        public string[] additionalImagePaths { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("places")]
        public string[] places { get; set; }
    }


    public class FaceMatchLookup
    {
        
        private string goingJSON = "going";
        private string goingpublicJSON = "going-public-profile";
        private string interestedJSON = "interested";

        string path = "attendingdata/";

        public Dictionary<string, PersonData> nameLookupTable;


        public List<PersonData> createAttendeeDataset()
        {
            
            List<PersonData> attendeeDataset;
            List<AdditionalPublicPersonData> attendeePublicDataset;
            List<PersonData> interestedDataset;

            nameLookupTable = new Dictionary<string, PersonData>();
            Dictionary<string, AdditionalPublicPersonData> publicDataLookupTable = new Dictionary<string, AdditionalPublicPersonData>();

            var goingJsonTextFile = Resources.Load<TextAsset>(path + goingJSON);
            var goingPublicJsonTextFile = Resources.Load<TextAsset>(path + goingpublicJSON);
            var interestedJsonTextFile = Resources.Load<TextAsset>(path + interestedJSON);

            attendeeDataset = JsonConvert.DeserializeObject<List<PersonData>>(goingJsonTextFile.text);
            attendeePublicDataset = JsonConvert.DeserializeObject<List<AdditionalPublicPersonData>>(goingPublicJsonTextFile.text); 
            interestedDataset = JsonConvert.DeserializeObject<List<PersonData>>(interestedJsonTextFile.text);

            foreach(PersonData interested in interestedDataset){
                interested.attendingEvent = false;
            }

            foreach (PersonData attendee in attendeeDataset)
            {
                attendee.attendingEvent = true;
            }

            attendeeDataset.AddRange(interestedDataset);

            foreach (AdditionalPublicPersonData publicAttendee in attendeePublicDataset)
            {
                publicDataLookupTable.Add(publicAttendee.name, publicAttendee);
            }

            foreach (PersonData attendee in attendeeDataset){

                AdditionalPublicPersonData temp;
                if(publicDataLookupTable.TryGetValue(attendee.name, out temp)){
                    attendee.additionalPublicPersonData = temp;
                    attendee.publicProfile = true;
                }
                nameLookupTable.Add(attendee.name, attendee);
            }

            return attendeeDataset;

        }
    }  
}

