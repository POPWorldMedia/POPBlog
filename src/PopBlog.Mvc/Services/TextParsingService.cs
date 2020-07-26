using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PopBlog.Mvc.Services
{
	public interface ITextParsingService
	{
		string CommentParse(string input);
		string UnrestrictedParse(string input);
		string ParseUrls(string input);
		string EscapeProhibitedTags(string input);
		string ParseLineBreaksToParagraphs(string input);
		string EnforceTagClosure(string input);
		string UnparseLineBreaks(string input);
	}

	public class TextParsingService : ITextParsingService
	{
		public string CommentParse(string input)
		{
			var output = EscapeProhibitedTags(input);
			output = ParseUrls(output);
			output = ParseLineBreaksToParagraphs(output);
			output = EnforceTagClosure(output);
			return output;
		}

		public string UnrestrictedParse(string input)
		{
			var output = ParseUrls(input);
			output = ParseLineBreaksToParagraphs(output);
			output = EnforceTagClosure(output);
			return output;
		}

		public string ParseUrls(string input)
		{
			var protolcPattern = new Regex(@"(?<![\]""\>=])(((news|(ht|f)tp(s?))\://)[\w\-\*]+(\.[\w\-/~\*]+)*/?)([\w\?=&/;\+%\*\:~,\.\-\$\|@#])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var wwwPattern = new Regex(@"(?<!(\]|""|//))(?<=\s|^)(w{3}(\.[\w\-/~\*]+)*/?)([\?\w=&;\+%\*\:~,\-\$\|@#])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var emailPattern = new Regex(@"(?<=\s|\])(?<!(mailto:|""\]))([\w\.\-_']+)@(([\w\-]+\.)+[\w\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var output = protolcPattern.Replace(input, match => String.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", match.Value));
			output = wwwPattern.Replace(output, match => String.Format("<a href=\"http://{0}\" target=\"_blank\">{0}</a>", match.Value));
			output = emailPattern.Replace(output, match => String.Format("<a href=\"mailto:{0}\">{0}</a>", match.Value));
			return output;
		}

		public string EscapeProhibitedTags(string input)
		{
			var output = Regex.Replace(input, @"</?(?!code|blockquote|a|b|i)\b[^>]*>", match => match.Value.Replace("<", "&lt;").Replace(">", "&gt;"), RegexOptions.IgnoreCase);
			return Regex.Replace(output, @"<(?=a)\b[^>]*>", match => match.Value.Replace("javascript:", String.Empty), RegexOptions.IgnoreCase);
		}

		public string ParseLineBreaksToParagraphs(string input)
		{
			var output = Regex.Replace(input, @"\r\n\r\n", "</p><p>");
			output = Regex.Replace(output, @"\n\n", "</p><p>");
			output = Regex.Replace(output, @"\r\n", "<br />");
			output = Regex.Replace(output, @"\n", "<br />");
			if (!output.StartsWith("<p>"))
				output = output.Insert(0, "<p>");
			if (!output.EndsWith("</p>"))
				output += "</p>";
			return output;
		}

		public string EnforceTagClosure(string input)
		{
			var stack = new Stack<string>();
			var offset = 0;
			var previousCloseMatchIndex = 0;
			var allMatch = new Regex(@"</?([a-z]+)(\s+\S+""\s*)*[^?!/>]*>", RegexOptions.Compiled); // exclude self-closing tags like <br /> or <img />
			var startMatch = new Regex(@"<([a-z]+)(\s+\S+""\s*)*[^?!/>]*>", RegexOptions.Compiled);
			var closeMatch = new Regex(@"</([a-z]+)\b[^>]*>", RegexOptions.Compiled);
			var allMatches = allMatch.Match(input);
			while (allMatches.Success)
			{
				var match = allMatches.Value;
				if (startMatch.IsMatch(match))
				{
					// starting tag, add to stack, go next
					var openTag = startMatch.Replace(match, "<$1>");
					stack.Push(openTag);
				}
				else if (closeMatch.IsMatch(match))
				{
					// ending tag
					if (stack.Count == 0 || !stack.Contains(match.Remove(1, 1)))
					{
						// no opening tag, insert one after last closer or at start
						var newTag = match.Remove(1, 1);
						input = InsertNewTag(input, newTag, ref offset, previousCloseMatchIndex);
					}
					else
					{
						while (match.Remove(1, 1) != stack.Peek())
						{
							// stack doesn't match closer
							var insertIndex = allMatches.Index + offset;
							var newTag = stack.Peek().Insert(1, "/");
							input = InsertNewTag(input, newTag, ref offset, insertIndex);
							stack.Pop();
						}
						stack.Pop();
					}
					previousCloseMatchIndex = allMatches.Index + allMatches.Length + offset;
				}
				allMatches = allMatches.NextMatch();
			}
			while (stack.Count > 0)
			{
				input = input + stack.Peek().Insert(1, "/");
				stack.Pop();
			}
			return input;
		}

		private string InsertNewTag(string input, string newTag, ref int offset, int insertIndex)
		{
			input = input.Insert(insertIndex, newTag);
			offset += newTag.Length;
			return input;
		}

		public string UnparseLineBreaks(string input)
		{
			if (input == null)
				return String.Empty;
			var graphBorder = new Regex(@"</p>\s*<p>");
			var output = graphBorder.Replace(input, "\r\n\r\n");
			var breakItem = new Regex("<br */>");
			output = breakItem.Replace(output, "\r\n");
			output = output.Replace("<p>", "");
			output = output.Replace("</p>", "");
			return output;
		}
	}
}