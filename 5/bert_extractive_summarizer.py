from summarizer import Summarizer

import spacy
from transformers import AutoConfig



body = """Proficient students understand that summarizing, identifying what is most important and restating the text (or other media) in your own words, is an important tool for college success.
#After all, if you really know a subject, you will be able to summarize it. If you cannot summarize a subject, even if you have memorized all the facts about it, you can be absolutely sure that you have not learned it. And, if you truly learn the subject, you will still be able to summarize it months or years from now.
#Proficient students may monitor their understanding of a text by summarizing as they read. They understand that if they can write a one- or two-sentence summary of each paragraph after reading it, then that is a good sign that they have correctly understood it. If they can not summarize the main idea of the paragraph, they know that comprehension has broken down and they need to use fix-up strategies to repair understanding."""

#body = 'こんにちは。僕の名前は藤田誠之です。'

custom_config = AutoConfig.from_pretrained('bert_base_japanese')
# custom_config.output_hidden_states=True
# custom_tokenizer = AutoTokenizer.from_pretrained('bert_base_japanese')
# custom_model = AutoModel.from_pretrained('bert_base_japanese', config=custom_config)


model = Summarizer()
#model = Summarizer(custom_model=custom_model, custom_tokenizer=custom_tokenizer)

print(model(body, num_sentences=3))