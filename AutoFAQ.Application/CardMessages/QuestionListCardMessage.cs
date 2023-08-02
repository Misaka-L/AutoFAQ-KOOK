using AutoFAQ.Core.CardMessages;
using AutoFAQ.Entity.Entity.FAQ;
using Kook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Application.CardMessages {
    public class QuestionListCardMessage : ICardMessage {
        public IGuild Guild;
        public QuestionEntity[] Questions;

        public QuestionListCardMessage(QuestionEntity[] questions, IGuild guild) {
            Questions = questions;
            Guild = guild;
        }

        public Card[] Build() {
            var card = new CardBuilder().WithTheme(CardTheme.Success).WithSize(CardSize.Large)
                .AddModule(new HeaderModuleBuilder().WithText(new PlainTextElementBuilder().WithContent($"服务器 \"{Guild.Name}\" 设置的问答列表")))
                .AddModule(new DividerModuleBuilder());

            string text = "";
            foreach (QuestionEntity question in Questions) {
                text += $"> ID:{question.Id} | {question.Regex} | {question.Answer}\n\n---\n";
            }

            card.AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder().WithContent(text)));

            return new Card[] {
                card.Build()
            };
        }
    }
}
