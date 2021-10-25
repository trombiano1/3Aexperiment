#encoding: UTF-8
import os,sys
import pathlib
import argparse
import ffmpeg #音声処理用
import speech_recognition as sr #音声認識
import MeCab #品詞分解/keyBERT
from keybert import KeyBERT #keyBERT

#============================================
# dependencies: ffmpeg, SpeechRecognition, MeCab, keyBERT, PyTorch (どれもpipで入る)
#
# 使い方は
# python3 project.py -h
# で出る
#
# -i 入力音声/動画の相対path(必須, 動画mp4でしか試してない)
# -s 開始時間と終了時間(秒)を指定できる(デフォルトは全部)
# -t をつけると音声認識の文章を保存する
#
# ex) $ python3 project.py -i kishida.mp4 -t -s 150 350
#       -> kishida.mp4を150秒~350秒で処理, 文章も保存
#============================================

# ffmpegで音声をカット----------------------------
def split_audio(path, time_section):
    print("Splitting audio....")
    directorypath = os.path.join(pathlib.Path(path).parent.resolve(),'project_data')
    os.mkdir(directorypath)
    audio_input = ffmpeg.input(path)
    info=ffmpeg.probe(path)
    original_duration = int(float(info['format']['duration']))
    if time_section == [-1,-1]:
        time_section = [0, original_duration]
    duration = time_section[1] - time_section[0]
    print("Duration: "+str(duration)+"s")
    paths = []
    for i in range (0,int(duration/170.0)+1):
        print("Saving audio",i+1,"of",int(duration/170.0)+1,"...")
        if i == int(duration/170.0):
            audio_cut = audio_input.audio.filter('atrim', duration=duration%170, start = i * 170 + time_section[0])
        else:
            audio_cut = audio_input.audio.filter('atrim', duration=170, start = i * 170 + time_section[0])
        audio_output = ffmpeg.output(audio_cut, os.path.join(directorypath,('sample'+str(i)+'.wav')),loglevel="quiet")
        paths.append(directorypath+'/sample'+str(i)+'.wav')
        ffmpeg.run(audio_output)
    return paths

def audio_to_txt(path):
    print("Recognizing audio....")
    AUDIO_FILE = path
    # use the audio file as the audio source
    r = sr.Recognizer()
    with sr.AudioFile(AUDIO_FILE) as source:
        audio = r.record(source)  # read the entire audio file

    result=r.recognize_google(audio, language='ja-JP')
    return result
    # try:
    #     print("Google Speech Recognition thinks you said " + result)
    # except sr.UnknownValueError:
    #     print("Google Speech Recognition could not understand audio")
    # except sr.RequestError as e:
    #     print("Could not request results from Google Speech Recognition service; {0}".format(e))
    
def audios_to_txt(paths):
    all_text = ""
    for i in range (0, len(paths)):
        print("Recognizing audio",i+1,"of",len(paths),"....")
        AUDIO_FILE = paths[i]
        # use the audio file as the audio source
        r = sr.Recognizer()
        with sr.AudioFile(AUDIO_FILE) as source:
            audio = r.record(source)  # read the entire audio file

        result=r.recognize_google(audio, language='ja-JP')
        all_text += result
    return all_text

# MeCab品詞分解--------------------------------------
def modify_tense(input_sentence):
    print("Recognizing PoS....")
    tokenizer = MeCab.Tagger("-Ochasen -d /usr/local/lib/mecab/dic/mecab-ipadic-neologd")
    tokenizer.parse("")
    node = tokenizer.parseToNode(input_sentence)
    keywords = []
    while node: #品詞ごとの処理
        if node.feature.split(",")[0] == u"名詞":
            keywords.append(node.surface)
        if node.feature.split(",")[0] == u"形容詞":
            #keywords.append(node.feature.split(",")[6]) #原形
            keywords.append(node.surface)
        elif node.feature.split(",")[0] == u"動詞":
            #keywords.append(node.feature.split(",")[6]) #原形
            keywords.append(node.surface)
        else:
            keywords.append(node.surface)
        node = node.next
    return ''.join(keywords)

def pos_analysis(input_sentence):
    # 品詞分解 
    tokenizer = MeCab.Tagger("-Ochasen -d /usr/local/lib/mecab/dic/mecab-ipadic-neologd")
    tokenizer.parse("")
    node = tokenizer.parseToNode(input_sentence)
    while node:
        #単語を取得
        word = node.surface
        #品詞を取得
        pos = node.feature.split(",")[1]
        print('{0} , {1}'.format(word, pos))
        #次の単語に進める
        node = node.next

# keyBERT で抽出 --------------------------------------
def keyword_extraction(input_sentence):
    print("Extracting keywords...")
    words = MeCab.Tagger("-Owakati").parse(input_sentence)
    # MeCabで分かち書き
    model = KeyBERT('paraphrase-multilingual-mpnet-base-v2')
    #print(model.extract_keywords(words, top_n = 20, keyphrase_ngram_range=(1, 1)))
    return model.extract_keywords(words, top_n = 20, keyphrase_ngram_range=(1, 1))

parser = argparse.ArgumentParser()
parser.add_argument('-i', metavar='INPUT_FILE_PATH',action='store', type=pathlib.Path, dest='input_path', help='input file path',required=True)
parser.add_argument('-t', action='store_true', dest='save_transcript')
#parser.add_argument('-t', metavar='SAVE_TRANSCRIPT', action='store_true', dest='save_transcript', help='save transcript as a txt file')
#parser.add_argument('-m', metavar='NUMBER_OF_SOUNDBITES', action='store', dest='n_soundbites', type=int, default=5, help='maximum number of soundbites')
parser.add_argument('-s', metavar=['START_SECOND','END_SECOND'], action='store', type=int, nargs=2, dest='time_section')

args = parser.parse_args()
input_path = args.input_path
#n_soundbites = int(args.n_soundbites)
save_transcript = args.save_transcript
if args.time_section == None:
    time_section = [-1,-1]
else:
    time_section = args.time_section

if time_section[0] > time_section[1]:
    exit(1)

paths = split_audio(input_path, time_section)
speech_recognition_text = audios_to_txt(paths)
if save_transcript:
    f = open(os.path.join(pathlib.Path(input_path).parent.resolve(),"project_data","TRANSCRIPT.txt"), "x")
    f.write(speech_recognition_text)
    f.close()
    print("Saved transcript to",os.path.join(pathlib.Path(input_path).parent.resolve(),"project_data","TRANSCRIPT.txt"))
tense_modified_text = modify_tense(speech_recognition_text)
extracted_keywords = keyword_extraction(tense_modified_text)
print(extracted_keywords)