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
# -m 5 とすると最大で170s * 5 秒分の音声を処理する(空欄なら10) 
#    放っとくと1時間分とかやり出しそうなので上限を設定
# -t をつけると音声認識の文章を保存する
#============================================

# ffmpegで音声をカット----------------------------
def split_audio(path, themax=10):
    print("Splitting audio....")
    directorypath = os.path.join(pathlib.Path(path).parent.resolve(),'project_data')
    os.mkdir(directorypath)
    audio_input = ffmpeg.input(path)
    info=ffmpeg.probe(path)
    duration = int(float(info['format']['duration']))
    print("Duration: "+str(duration)+"s")
    paths = []
    for i in range (0,min(int(duration/170.0)+1,themax)):
        print("Saving audio",i+1,"of",min(int(duration/170.0)+1,themax),"...")
        audio_cut = audio_input.audio.filter('atrim', duration=170, start = i * 170)
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
parser.add_argument('-m', metavar='NUMBER_OF_SOUNDBITES', action='store', dest='n_soundbites', type=int, default=5, help='maximum number of soundbites')

args = parser.parse_args()
input_path = args.input_path
n_soundbites = int(args.n_soundbites)
save_transcript = args.save_transcript

paths = split_audio(input_path,n_soundbites)
speech_recognition_text = audios_to_txt(paths)
if save_transcript:
    f = open(os.path.join(pathlib.Path(input_path).parent.resolve(),"project_data","TRANSCRIPT.txt"), "x")
    f.write(speech_recognition_text)
    f.close()
    print("Saved transcript to",os.path.join(pathlib.Path(input_path).parent.resolve(),"project_data","TRANSCRIPT.txt"))
tense_modified_text = modify_tense(speech_recognition_text)
extracted_keywords = keyword_extraction(tense_modified_text)
print(extracted_keywords)