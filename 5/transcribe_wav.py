filepath = "南アジア.wav"     #Input audio file path
output_filepath = "thetext.txt" #Final transcript path
bucketname = "callsaudiofiles" #Name of the bucket created in the step before

# Import libraries
from pydub import AudioSegment
import io
import os
from google.cloud import speech
from google.cloud.speech import enums
from google.cloud.speech import types
import wave
from google.cloud import storage