using Kook;
using Kook.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Core.CardMessages {
    public class CommandErrorCardMessage : ICardMessage {
        public CommandInfo? Command;
        public ICommandContext Context;
        public IResult Result;

        public CommandErrorCardMessage(CommandInfo? command, ICommandContext context, IResult result) {
            Command = command;
            Context = context;
            Result = result;
        }

        public Card[] Build() {
            return new Card[] {
                new CardBuilder()
                .WithSize(CardSize.Large).WithTheme(CardTheme.Danger)
                .AddModule(new ContextModuleBuilder().AddElement(new KMarkdownElementBuilder().WithContent(DateTimeOffset.Now.ToString("G"))))
                .AddModule(new HeaderModuleBuilder().WithText(new PlainTextElementBuilder().WithContent($"执行命令 {Command?.Name} 时发生错误")))
                .AddModule(new DividerModuleBuilder())
                .AddModule(new SectionModuleBuilder().WithText(new ParagraphStructBuilder().WithColumnCount(3)
                .AddField(new KMarkdownElementBuilder().WithContent($"**类型**\n`{Result.Error}`"))
                .AddField(new KMarkdownElementBuilder().WithContent($"**模块**\n`{Command?.Module.Name}`"))))
                .AddModule(new SectionModuleBuilder().WithText(new KMarkdownElementBuilder().WithContent($"**详细信息**\n```\n{Result.ErrorReason}\n```")))
                .Build()
            };
        }
    }
}
