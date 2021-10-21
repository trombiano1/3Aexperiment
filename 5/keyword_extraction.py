# coding: UTF-8
from keybert import KeyBERT
import MeCab
doc1copy = """
さあ 本日は純物質と混合物の状態変化について学んで行きましょう"""

doc1 = 'はいはい 代100代内閣総理大臣に指名されました 岸田文雄です 自由民主党と公明党の連立による新たな 内閣が発足 いたしました 咳 職責を果たさリオ 全身全霊で取り組んで参ります まず 新型 コロナウイルスにより亡くなられた方とご家族の皆様に心からお悔やみを申し上げますとともに厳しい 闘病生活を送っておられる 多くの方々にお見舞いを申し上げさせて頂きたいと思います また 医療保険ありは海軍こういった この現場の最前線で奮闘されている方々や 感染対策に協力してくださっている事業者の方々 そして国民の皆様方に深く感謝を申し上げさせて頂きます 新型 コロナとの戦いは続いています 私の内閣ではまず チキンカツ 最優先の課題であります 新型 コロナ 対策 この 万全を期してまいります 国民に納得感を持ってもらえる 丁寧な説明を行うこと そして 常に最悪の事態を想定して対応することを基本としてあります また新型 コロナによって大きな影響を受けておられる方々を支援するために 速やかには 経済対策を作成してまいります その上で 私が私が目指すのは新しい資本主義の実現 ですわー 国'

tokenizer = MeCab.Tagger("-Ochasen -d /usr/local/lib/mecab/dic/mecab-ipadic-neologd")
tokenizer.parse("")
node = tokenizer.parseToNode(doc1)
keywords = []

while node:
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

# MeCabで分かち書き
words = MeCab.Tagger("-Owakati").parse(combined)

model = KeyBERT('paraphrase-multilingual-mpnet-base-v2')

print(model.extract_keywords(words, top_n = 20, keyphrase_ngram_range=(1, 1)))

# 品詞分解
while node:
    #単語を取得
    word = node.surface
    #品詞を取得
    pos = node.feature.split(",")[1]
    print('{0} , {1}'.format(word, pos))
    #次の単語に進める
    node = node.next