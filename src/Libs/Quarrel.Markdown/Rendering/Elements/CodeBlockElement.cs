﻿// Quarrel © 2022

using ColorCode;
using Quarrel.Markdown.Parsing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Quarrel.Markdown
{
    public sealed class CodeBlockElement : MarkdownBlockElement
    {
        private readonly CodeBlock _codeBlock;
        
        internal CodeBlockElement(CodeBlock codeBlock) : base(codeBlock)
        {
            this.DefaultStyleKey = typeof(CodeBlockElement);
            _codeBlock = codeBlock;
        }

        protected override void Render(RichTextBlock richBlock)
        {
            richBlock.Blocks.Clear();
            richBlock.Blocks.Add(Paragraph);

            if (!string.IsNullOrEmpty(_codeBlock.Language) && Languages.FindById(_codeBlock.Language) is { } language)
            {
                var a = new RichTextBlockFormatter(ElementTheme.Dark);
                a.FormatInlines(_codeBlock.Content, language, Paragraph.Inlines);
            }
            else
            {
                Paragraph.Inlines.Add(new Run() { Text = _codeBlock.Content });
            }
        }
    }
}
