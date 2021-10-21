import speech_recognition as sr
import MeCab
from keybert import KeyBERT

# ffmpegで音声をカット----------------------------
#import ffmpeg

#audio_input = ffmpeg.input('./audio_samples/kishida/kishida.mp4')
#audio_cut = audio_input.audio.filter('atrim', duration=170)
#audio_output = ffmpeg.output(audio_cut, './audio_samples/kishida/sample1.wav')
#ffmpeg.run(audio_output)


def audio_to_txt():
    print("Recognizing audio....")

    AUDIO_FILE = "./audio_samples/kishida/sample3.wav"

    # use the audio file as the audio source
    r = sr.Recognizer()
    with sr.AudioFile(AUDIO_FILE) as source:
        audio = r.record(source)  # read the entire audio file

    result=r.recognize_google(audio, language='ja-JP')
    print("Audio recognition: done ------------")
    print(result)
    return result

        # try:
        #     print("Google Speech Recognition thinks you said " + result)
        # except sr.UnknownValueError:
        #     print("Google Speech Recognition could not understand audio")
        # except sr.RequestError as e:
        #     print("Could not request results from Google Speech Recognition service; {0}".format(e))

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

    combined = ''.join(keywords)
    print("PoS recognition: done")
    return combined

# keyBERT で抽出 --------------------------------------
def keyword_extraction(input_sentence):
    print("Extracting keywords...")
    words = MeCab.Tagger("-Owakati").parse(input_sentence)
    model = KeyBERT('paraphrase-multilingual-mpnet-base-v2')
    print("Keyword extraction: done ------------")
    print(model.extract_keywords(words, top_n = 20, keyphrase_ngram_range=(1, 1)))

# MeCabで分かち書き
