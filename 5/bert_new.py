import torch
# GPUが使えれば利用する設定
device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")

import os
import urllib.request
import re
import csv
import tarfile

# データのダウンロード（カレントディレクトリに圧縮ファイルがダウンロードされる）
#urllib.request.urlretrieve("https://www.rondhuit.com/download/ldcc-20140209.tar.gz", "ldcc-20140209.tar.gz")

# ダウンロードした圧縮ファイルのパスを設定
tgz_fname = "ldcc-20140209.tar.gz" 
# 2つをニュースメディアのジャンルを選定
target_genre = ["it-life-hack", "sports-watch"] 
#処理をした結果を保存するファイル名 
tsv_fname = "all_text.tsv" 


# 処理部分-------
brackets_tail = re.compile('【[^】]*】$')
brackets_head = re.compile('^【[^】]*】')

def remove_brackets(inp):
    output = re.sub(brackets_head, '', re.sub(brackets_tail, '', inp))
    return output

def read_title(f):
    # 2行スキップ
    next(f)
    next(f)
    title = next(f) # 3行目を返す
    title = remove_brackets(title.decode('utf-8'))
    return title[:-1]

zero_fnames = []
one_fnames = []

with tarfile.open(tgz_fname) as tf:
    # 対象ファイルの選定
    for ti in tf:
        # ライセンスファイルはスキップ
        if "LICENSE.txt" in ti.name:
            continue
        if target_genre[0] in ti.name and ti.name.endswith(".txt"):
            zero_fnames.append(ti.name)
            continue
        if target_genre[1] in ti.name and ti.name.endswith(".txt"):
            one_fnames.append(ti.name)
    with open(tsv_fname, "w") as wf:
        writer = csv.writer(wf, delimiter='\t')
        # ラベル 0
        for name in zero_fnames:
            f = tf.extractfile(name)
            title = read_title(f)
            row = [target_genre[0], 0, '', title]
            writer.writerow(row)
        # ラベル 1
        for name in one_fnames:
            f = tf.extractfile(name)
            title = read_title(f)
            row = [target_genre[1], 1, '', title]
            writer.writerow(row)

import pandas as pd

# データの読み込み
df = pd.read_csv("all_text.tsv", 
                 delimiter='\t', header=None, names=['media_name', 'label', 'NaN', 'sentence'])

# データの確認
print(f'データサイズ： {df.shape}')
df.sample(10)

# データの抽出
sentences = df.sentence.values
labels = df.label.values

# 1. BERT Tokenizerを用いて単語分割・IDへ変換
## Tokenizerの準備
from transformers import BertJapaneseTokenizer
tokenizer = BertJapaneseTokenizer.from_pretrained('bert-base-japanese-whole-word-masking')