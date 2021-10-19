from keybert import KeyBERT
import MeCab

doc1 = """
今日から南アジアについて勉強してことにしたいと思いますそしてこの南あわじ行ったのは、一体どこに行ってるのか、こちらのツムで確認しましょう。はい、そうします。これ世界地図が出てきてね、みなみよしあこれを黄色くなって示してるわけですけども、もちろんあのインドを中心とする地域ね、こいつでありますが、中毒性昨日よりもちょっと北側のところに、ちょうどユーラシア大陸から南の方に突き出したインド半島を売ってないしててね。"""

# MeCabで分かち書き
words = MeCab.Tagger("-Owakati").parse(doc1)

model = KeyBERT('paraphrase-multilingual-MiniLM-L12-v2')

print(model.extract_keywords(words, top_n = 100, keyphrase_ngram_range=(1, 1)))