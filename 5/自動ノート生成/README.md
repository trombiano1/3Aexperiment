# 自動ノート生成
### 7班　宮城賢太, 今泉謙斗, 藤田誠之


課題5「人工知能演習」の7班で作成した動画から自動でノート用のパワーポイントを作成するプログラムです.

# 準備 Requirements
SpeechRecognitionのインストール
```shell
$ pip3 install SpeechRecognition
$ pip3 install pyaudio
$ pip3 install google-api-python-client
```

MeCabのインストール
```shell
$ brew install mecab
$ brew install mecab-ipadic
$ pip3 install mecab-python3
$ pip3 install unidic-lite
```

keyBERTのインストール
```shell
$ pip3 install keybert
```

その他の依存性
```
opencv-python
pytest-shutil
pathlib
ffmpeg
ffmpeg-python
python-pptx
```

# 使い方 Usage
```shell
$ python3 autonotes.py -i 入力ファイルパス [OPTIONS]
```
Options: 
```
ヘルプ -h --help
入力言語 -l "ja"(デフォルト)か"en"
時間指定 -s [start second, end second]
書き起こしを保存 -t
キーワードを表示 -k
```

動作確認済の動画をGoogle Driveにアップロードしたので以下からダウンロードしてください(ECCSアカウントが必要です).\
[岸田総理大臣就任会見](https://drive.google.com/drive/folders/1Vu2pPqBfBGqJIB_acbuQybYWAkf4Gp7w?usp=sharing)\
[Obama Victory Speech](https://drive.google.com/file/d/1KmieAwh_7aNk7-bx2Ac9HIwK4hBAQEMf/view?usp=sharing)

### 例
```shell
$ python3 autonotes.py -i /path/to/岸田総理大臣就任会見.mp4 -s 0 896
```
```shell
$ python3 autonotes.py -i /path/to/Obama\ Victory\ Speech.mp4 -l en
```

# Attributes
[Speech Recognition](https://pypi.org/project/SpeechRecognition/)\
[MacでpythonのSpeechRecognitionを使って音声認識](https://qiita.com/seigot/items/62a85f1a561bb820532a)\
[speech_recognition](https://github.com/Uberi/speech_recognition/blob/master/examples/audio_transcribe.py)\
[KeyBERTでキーフレーズ抽出を試してみる](https://crieit.net/posts/KeyBERT)\
[自然言語処理モデル（BERT）を利用した日本語の文章分類](https://qiita.com/takubb/items/fd972f0ac3dba909c293)