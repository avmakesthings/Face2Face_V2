# Copyright 2017 Amazon.com, Inc. or its affiliates. All Rights Reserved.
# Licensed under the Amazon Software License (the "License"). You may not use this file except in compliance with the License. A copy of the License is located at
#     http://aws.amazon.com/asl/
# or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the specific language governing permissions and limitations under the License.
# Modified by Anastasia Victor to run with Unity 


from __future__ import print_function
import base64
import datetime
import time
import decimal
import uuid
import json
import cPickle
import boto3
import pytz
from pytz import timezone
from copy import deepcopy
import io



def load_config():
    '''Load configuration from file.''' 
    with open('imageprocessor-params.json', 'r') as conf_file:
        conf_json = conf_file.read()
        return json.loads(conf_json)

def convert_ts(ts, config):
    '''Converts a timestamp to the configured timezone. Returns a localized datetime object.'''
    #lambda_tz = timezone('US/Pacific')
    tz = timezone(config['timezone'])
    utc = pytz.utc
    utc_dt = utc.localize(datetime.datetime.utcfromtimestamp(ts))
    localized_dt = utc_dt.astimezone(tz)
    return localized_dt

def process_image(event, context):

    #Initialize clients
    rekog_client = boto3.client('rekognition')
    kinesis_client = boto3.client('kinesis')
    dynamodb_client = boto3.client('dynamodb')

    #Load config
    config = load_config()
    output_kinesis_stream = config["output_kinesis_stream"]
    rekog_attributes = ['ALL']
    rekog_features_blacklist = ("Landmarks", "Emotions", "Pose", "Quality", "BoundingBox", "Confidence")
    
    # Iterate on frames fetched from Kinesis
    for record in event['Records']:
        item = None
        
        try:
            #from Unity
            decoded = base64.b64decode(record['kinesis']['data'])
            frame_package =json.loads(decoded)
    
            img_bytes = base64.decodestring(frame_package["ImageBytes"])
            approx_capture_ts = frame_package["ApproximateCaptureTime"]
    
            frame_count = frame_package["FrameCount"]
            now_ts = time.time()
            frame_id = str(uuid.uuid4())
            processed_timestamp = decimal.Decimal(now_ts)
            approx_capture_timestamp = str(approx_capture_ts)
    
            now = convert_ts(now_ts, config)
            year = now.strftime("%Y")
            mon = now.strftime("%m")
            day = now.strftime("%d")
            hour = now.strftime("%H")
            
            detectfacesresponse = rekog_client.detect_faces(
                Image={
                    'Bytes': img_bytes
                },
                Attributes=rekog_attributes,
            )
            
            faceMatchName = ""
            faceMatches = []
            
            searchfacesresponse = rekog_client.search_faces_by_image(
                CollectionId='gray-area-event',
                Image={'Bytes':img_bytes}                                       
            )
            print('just got search face response' + str(searchfacesresponse['FaceMatches']))
            
            faceMatches = searchfacesresponse['FaceMatches']

            if(len(searchfacesresponse['FaceMatches']) > 0):
                match = searchfacesresponse['FaceMatches'][0]
                
                print ("Face match found, FaceID: {0} and Confidence {1}".format(match['Face']['FaceId'],match['Face']['Confidence']) )
                
                face = dynamodb_client.get_item(
                    TableName='gray_area_collection',  
                    Key={'RekognitionId': {'S': match['Face']['FaceId']}}
                    )
                
                if 'Item' in face:
                    faceMatchName = face['Item']['FullName']['S']
                    print (face['Item']['FullName']['S'])
                else:
                    print ('no match found in person lookup')    

            
            print("Populating item")    
            item = {
                'frame_id': frame_id,
                'processed_timestamp' : str(processed_timestamp),
                'approx_capture_timestamp' : str(approx_capture_timestamp),
                'rekog_face_details' : detectfacesresponse['FaceDetails'],
                'rekog_face_matches' : faceMatches,
                'dynamodb_face_match_name' : faceMatchName,
                'rekog_orientation_correction' : 
                    detectfacesresponse['OrientationCorrection'] 
                    if 'OrientationCorrection' in detectfacesresponse else 'ROTATE_0'
            }
            
        except Exception as e:
            print("Error:  "+ str(e))
    
        # Write to stream
        
        if item is not None:
            print("write to stream")
            response = kinesis_client.put_record(
                StreamName=output_kinesis_stream,
                Data=json.dumps(item),
                PartitionKey="partitionkey"
            )
        
    print('Successfully processed {} records.'.format(len(event['Records'])))
    return

def handler(event, context):
    return process_image(event, context)
