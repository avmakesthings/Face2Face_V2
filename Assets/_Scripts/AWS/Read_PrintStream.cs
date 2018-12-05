using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FaceData;
using System.Text;

public class Read_PrintStream : MonoBehaviour {
    
	public GameObject AWSClientObject;
    public Text nameText;
    public Text ageText;
    public Text genderText;
    public Text profileURLText;
    public Text placeFrom;
    public Text placesWorked;
	public Text emotionText;
    //public Text mustacheText;
    //public Text glassesText;
    //public Text beardText;
    public RawImage testTexture;


    public Dictionary<string, PersonData> nameLookUp;

    public RectTransform infoLayout;
    string emotionStr;
    private List<PersonData> attendeeDataset;


	// Use this for initialization
	void  Start() {
		StartCoroutine("TestClient");

        FaceMatchLookup faceMatchLookup = new FaceMatchLookup();
        attendeeDataset = faceMatchLookup.createAttendeeDataset();
        nameLookUp = faceMatchLookup.nameLookupTable;
	}

	void HandleError(Exception e){
		Debug.LogError(e);
	}



	IEnumerator TestClient(){
		yield return null;// new WaitForSeconds(1f);
		try{
			// Test POC AWS client
			AWSClient awsClient = AWSClientObject.GetComponent<AWSClient>();
			string streamName = "AmazonRekognitionStreamOut";


			awsClient.ReadStream(streamName, (response)=>{
				List<Amazon.Kinesis.Model.Record> records = response.Records;
				foreach(Amazon.Kinesis.Model.Record awsRecord in records){
					
					try{

                        resetDataFields("");
						string recordString = Encoding.ASCII.GetString(awsRecord.Data.ToArray());
						Rekog.Record record = Rekog.Record.Deserialize(recordString);

                        if(record.dynamodb_face_match_name != ""){
                            Debug.Log(String.Format("Matched name is {0} and confidence is {1}", record.dynamodb_face_match_name, record.rekog_face_matches[0].face.Confidence));
                            lookupMatchedUserData(record.dynamodb_face_match_name);
                            nameText.text = record.dynamodb_face_match_name;
                        }else {
                            Debug.Log("no match");
                        }

						if(record.rekog_face_details.Count > 0 ){
							printAge(record.rekog_face_details[0].AgeRange.Low, record.rekog_face_details[0].AgeRange.High);
							printGender(record.rekog_face_details[0].Gender.Value, record.rekog_face_details[0].Gender.Confidence);
							
							emotionStr = "";
							foreach(Rekog.Emotion emotion in record.rekog_face_details[0].Emotions){
								string s = printEmotion(emotion.Type,emotion.Confidence);
								emotionStr = emotionStr + s;
							}

							//emotionText.text = emotionStr;
							//printConfidence(record.rekog_face_details[0].Mustache.Confidence, record.rekog_face_details[0].Mustache.Value,mustacheText);
							//printConfidence(record.rekog_face_details[0].Beard.Confidence,record.rekog_face_details[0].Beard.Value ,beardText);
							//printConfidence(record.rekog_face_details[0].Eyeglasses.Confidence, record.rekog_face_details[0].Eyeglasses.Value, glassesText);


						}


					} catch(Exception e){
						HandleError(e);
					}

				}
			});
		} catch(Exception e){
			HandleError(e);
		}
		
	}

	void printAge(float lowAge, float highAge ){
		string s = String.Format("{0} - {1}", (int)lowAge, (int)highAge);
		ageText.text = s;
	}

	void printGender(string myGender, float confidence){
		string s = String.Format("{0}% {1}", (int)confidence, myGender);
		genderText.text = s;
	}

	void printConfidence(float confidence, bool value, Text text){
		string s;
		if(value){
			s = confidence.ToString("n2")+ "% Yes";
		}else{
			s = confidence.ToString("n2") + "% No";
		}
		text.text = s;
	}

	string printEmotion(string myEmotion, float confidence){
		string s = String.Format("{0}% {1} \n", confidence.ToString("n2"), myEmotion);
		return s;
	}


    void lookupMatchedUserData(string faceMatchName)
    {
        
        PersonData matchedFaceData = new PersonData();
        string imgFolderPath;

        if (nameLookUp.TryGetValue(faceMatchName, out matchedFaceData))
        {
            profileURLText.text = matchedFaceData.profileURL;

            if(matchedFaceData.attendingEvent){
                imgFolderPath = "going_files/";
            }else{
                imgFolderPath = "interested_files/";
            }
            string profileImagePath = matchedFaceData.profileImagePath;
            string trimmedImgPath = imgFolderPath + System.IO.Path.GetFileNameWithoutExtension(profileImagePath.Remove(0, 1));
            Texture2D texture = Resources.Load(trimmedImgPath) as Texture2D;
            testTexture.texture = texture;

            if(matchedFaceData.publicProfile){
                placeFrom.text = String.Join(" / ",matchedFaceData.additionalPublicPersonData.places);
                placesWorked.text = String.Join(" / ", matchedFaceData.additionalPublicPersonData.education);
            }
        }
        else
        {
            Debug.Log("There was no matched face data");
            profileURLText.text = "";
        }
    }

    public void resetDataFields(string resetString){
        
        nameText.text = resetString;
        ageText.text= resetString;
        genderText.text = resetString;
        profileURLText.text = resetString;
        placeFrom.text = resetString;
        placesWorked.text = resetString;
        testTexture.texture = null;
    }

}
