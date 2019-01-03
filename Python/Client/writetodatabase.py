

# - name
# - Blurb
# - Studies
# - Went to
# - Lives in
# - From
# need to figure out if i can get the additional photos 

import boto3
import json


s3 = boto3.resource('s3')
rootDirectory = "/Users/AV/Desktop/scrape-dls/"
goingJSON = "going.json"
goingPublicProfileJSON = "going-public-profile.json"
interstedJSON = "interested.json"



def load_json(filepath):
    '''Load json from file.''' 
    with open(filepath, 'r') as jsonFile:
        jsonString = jsonFile.read()
        return json.loads(jsonString)

def upload_image_to_s3(imagepath, metadata, s3path):
      try:
            file = open(imagepath,'rb')
      except Exception as inst:
            print("Something went wrong!")
            print(inst)
      # print s3path
      object = s3.Object('grayareadatabase','index/'+ s3path)
      ret = object.put(Body=file, Metadata=metadata)


def createLookupTableByName(filename):
      additionalJSONData = load_json(rootDirectory+filename)
      table = {}
      for item in additionalJSONData:
            key =  item['name']
            value =  item['imagePaths']
            table[key] = value
      
      return table



def writeToAWS(filename):
      jsonData = load_json(rootDirectory+filename)
      lookupTable = createLookupTableByName(goingPublicProfileJSON)

      for item in jsonData:
            name = item['name']
            metadata = {
                  'FullName': item['name'].encode('ascii', 'ignore'),
                  'URL': item['profileURL'].encode('ascii', 'ignore'),
            }

            if name in lookupTable:
                  for imagePath in lookupTable[name]:
                        s3path = "/".join(imagePath.split('/')[-3:])
                        upload_image_to_s3(rootDirectory+imagePath, metadata, s3path)

            imagePath = item['imagePath']
            s3path = "/".join(imagePath.split('/')[-2:])
            upload_image_to_s3(rootDirectory+imagePath, metadata, s3path)

                  
                  
      





writeToAWS(interstedJSON)

# Get list of objects for indexing
# images=[('TestImages/anastasia1.jpg','Anastasia Victor','https://www.facebook.com/anastasia.victor'),
#       ('TestImages/anastasia2.jpg','Anastasia Victor', 'https://www.facebook.com/anastasia.victor'),
#       ('TestImages/john1.jpg','John Faichney','https://www.facebook.com/john.victor.faichney'),
#       ('TestImages/john2.jpg','John Faichney','https://www.facebook.com/john.victor.faichney')
#       ]


# Iterate through list to upload objects to S3   
# for image in images:
#     file = open(image[0],'rb')
#     object = s3.Object('grayareadatabase','index/'+ image[0])
#     ret = object.put(Body=file,
#                     Metadata={'FullName':image[1],
#                                 'URL': image[2]
#                         }
#                     )
