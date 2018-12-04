

# - name
# - Blurb
# - Studies
# - Went to
# - Lives in
# - From
# need to figure out if i can get the additional photos 

import boto3

s3 = boto3.resource('s3')

# Get list of objects for indexing
images=[('TestImages/anastasia1.jpg','Anastasia Victor','https://www.facebook.com/anastasia.victor'),
      ('TestImages/anastasia2.jpg','Anastasia Victor', 'https://www.facebook.com/anastasia.victor'),
      ('TestImages/john1.jpg','John Faichney','https://www.facebook.com/john.victor.faichney'),
      ('TestImages/john2.jpg','John Faichney','https://www.facebook.com/john.victor.faichney')
      ]

# Iterate through list to upload objects to S3   
for image in images:
    file = open(image[0],'rb')
    object = s3.Object('grayareadatabase','index/'+ image[0])
    ret = object.put(Body=file,
                    Metadata={'FullName':image[1],
                                'URL': image[2]
                        }
                    )