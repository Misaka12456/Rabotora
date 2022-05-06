#pragma warning disable 8618
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static Rabotora.Core.Rsf.RabotoraScriptFormatParser;

namespace Rabotora.Core.Rsf
{
	public interface IRawRsfValue
	{

	}

	public class RawRsfInt : IRawRsfValue
	{
		public int Data;

		/// <summary>
		/// 将整数值转换为对应的Rabotora Script 元整数(Int) <see cref="RawRsfInt"/> 。
		/// </summary>
		/// <param name="data">要转换的整数值。</param>
		public static explicit operator RawRsfInt(int data)
		{
			return new()
			{
				Data = data
			};
		}
	}

	public class RawRsfFloat : IRawRsfValue
	{
		public float Data;

		/// <summary>
		/// 将浮点值转换为对应的Rabotora Script 元浮点数(Float) <see cref="RawRsfFloat"/> 。
		/// </summary>
		/// <param name="data">要转换的浮点值。</param>
		public static explicit operator RawRsfFloat(float data)
		{
			return new()
			{
				Data = data
			};
		}
	}

	public class RawRsfWord : IRawRsfValue
	{
		public string Data = string.Empty;

		/// <summary>
		/// 将字符串转换为对应的Rabotora Script 元文本(Word) <see cref="RawRsfWord"/> 。
		/// </summary>
		/// <param name="data">要转换的字符串。</param>
		public static explicit operator RawRsfWord(string data)
		{
			return new()
			{
				Data = data
			};
		}
	}

	public class RawRsfPlainString : IRawRsfValue
	{
		public string Data = string.Empty;

		/// <summary>
		/// 将字符串转换为对应的Rabotora Script 元纯文本(Plain String) <see cref="RawRsfPlainString"/> 。
		/// </summary>
		/// <param name="data">要转换的字符串。</param>
		public static explicit operator RawRsfPlainString(string data)
		{
			return new()
			{
				Data = data
			};
		}
	}

	public interface IRawRsfCmd
	{

	}

	public class RawRsfReturn : IRawRsfCmd
	{
		public IRawRsfValue ReturnValue;
	}

	public class RawRsfFunctionCall : IRawRsfCmd
	{
		public List<RawRsfWord> FunctionNames = new List<RawRsfWord>();
		public List<IRawRsfValue> Arguments = new List<IRawRsfValue>();
	}

	public class RawRsfVariableSet : IRawRsfCmd
	{
		public bool IsValueFromVariable { get => FromVariable != null; }
		public RawRsfWord TargetVariable;
		public RawRsfWord? FromVariable = null;
		public IRawRsfValue? FromValue = null;
	}

	public class RawRsfVariableDefinition : IRawRsfCmd
	{
		public RawRsfWord VariableType;
		public RawRsfWord VariableName;
		public IRawRsfValue? DefaultValue = null;
	}

	public class RawRsfFunction
	{
		public bool IsStaticFunction = false;
		public RawRsfWord FunctionName;
		public RawRsfWord Visibility;
		public RawRsfWord? ReturnValue = null;
		public List<Tuple<RawRsfWord, RawRsfWord>> Arguments;
		public List<IRawRsfCmd> Content;
	}

	public class RawRsfClass
	{
		public bool IsStaticClass = false;
		public RawRsfWord ClassName;
		public RawRsfWord Visibility;
		public List<RawRsfFunction> Functions;
	}

	public class RawRsfScript
	{
		public string ScriptName = string.Empty;
		public List<RawRsfClass> Classes = new List<RawRsfClass>();
		public List<string> Warning = new List<string>();
		public List<string> Error = new List<string>();
	}

	public class RabotoraScriptFormat
	{
		public static RawRsfScript ParseScript(string scriptData)
		{
			var script = new RawRsfScript();
			var stream = new AntlrInputStream(scriptData);
			var lexer = new RabotoraScriptFormatLexer(stream);
			var lexerErrorListener = new RsfErrorListener<int>(script);
			lexer.RemoveErrorListeners();
			lexer.AddErrorListener(lexerErrorListener);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RabotoraScriptFormatParser(tokens)
			{
				BuildParseTree = true
			};
			var parserErrorListener = new RsfErrorListener<IToken>(script);
			parser.RemoveErrorListeners();
			parser.AddErrorListener(parserErrorListener);
			var tree = parser.script();
			if (script.Error.Count == 0)
			{
				ParseTreeWalker.Default.Walk(new RsfTypeChecker(script), tree);
			}
			string warningStr = string.Join('\n', script.Warning);
			string errorStr = string.Join('\n', script.Error);
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(warningStr);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(errorStr);
			Console.ResetColor();
			Console.WriteLine($" {script.Error} error(s), {script.Warning} warning(s)");
			return script;
		}
	}

	public class RsfTypeChecker : RabotoraScriptFormatBaseListener
	{
		private RawRsfScript Script;

		public RsfTypeChecker(RawRsfScript script)
		{
			Script = script;
		}

		public override void ExitValue([NotNull] ValueContext context)
		{
			if (context.Int() != null)
			{
				if (int.TryParse(context.Int().GetText(), out int data))
				{
					context.Value = (RawRsfInt)data;
				}
				else
				{
					Script.Warning.Add($"Warning [RS1001]: Non-integer value for integer argument. (Line {context.Int().Symbol.Line}, Position {context.Int().Symbol.Column + 1})");
				}
			}
			else if (context.Float() != null)
			{
				if (float.TryParse(context.Float().GetText(), out float data))
				{
					context.Value = (RawRsfFloat)data;
				}
				else
				{
					Script.Warning.Add($"Warning [RS1002]: Non-float value for float argument. (Line {context.Float().Symbol.Line}, Position {context.Float().Symbol.Column + 1})");
				}
			}
			else if (context.Word() != null)
			{
				string data = context.Word().GetText();
				if ((data.StartsWith('"') || data.StartsWith('\'')) && (data.EndsWith('"') || data.EndsWith('\'')))
				{
					Script.Warning.Add($"Warning [RS1003]: Plain string value for word-string argument. (Line {context.Float().Symbol.Line}, Position {context.Float().Symbol.Column + 1})");
				}
				else
				{
					context.Value = (RawRsfWord)data;
				}
			}
			else if (context.plainString() != null)
			{
				string data = context.Word().GetText();
				if ((data.StartsWith('"') || data.StartsWith('\'')) && (data.EndsWith('"') || data.EndsWith('\'')))
				{
					if (data.StartsWith('\''))
					{
						data = data.Trim('\'');
					}
					else if (data.StartsWith('"'))
					{
						data = data.Trim('"');
					}
					context.Value = (RawRsfPlainString)data;
				}
				else
				{
					Script.Warning.Add($"Warning [RS1004]: Word string value for plain-string argument. (Line {context.Float().Symbol.Line}, Position {context.Float().Symbol.Column + 1})");
				}
			}
		}

		public override void ExitScript([NotNull] ScriptContext context)
		{
			foreach (var @class in context.body().classSegment())
			{
				if (@class.classDef() == null) continue;
				var classDef = @class.classDef();
				var classData = new RawRsfClass()
				{
					ClassName = (RawRsfWord)classDef.Word().GetText(),
					IsStaticClass = classDef.StaticKeyWord() != null && bool.TryParse(classDef.StaticKeyWord().GetText(), out bool isStaticClass) && isStaticClass
				};
				RawRsfWord classVisibility = (RawRsfWord)"internal";
				if (classDef.PublicKeyWord() != null)
				{
					classVisibility = (RawRsfWord)classDef.PublicKeyWord().GetText();
				}
				else if (classDef.PrivateKeyWord() != null)
				{
					classVisibility = (RawRsfWord)(classDef.PrivateKeyWord().GetText());
				}
				else if (classDef.InternalKeyWord() != null)
				{
					classVisibility = (RawRsfWord)(classDef.InternalKeyWord().GetText());
				}
				classData.Visibility = classVisibility;
				var funcList = new List<RawRsfFunction>();
				foreach (var func in @class.funcSegment())
				{
					if (func.funcDef() == null) continue;
					var funcDef = func.funcDef();
					var funcData = new RawRsfFunction()
					{
						FunctionName = (RawRsfWord)((funcDef.ActionKeyWord() != null) ? funcDef.Word().ElementAt(0).GetText() : funcDef.Word().ElementAt(1).GetText()),
						IsStaticFunction = funcDef.StaticKeyWord() != null && bool.TryParse(funcDef.StaticKeyWord().GetText(), out bool isStaticFunc) && isStaticFunc,
						ReturnValue = (funcDef.ActionKeyWord() == null) ? (RawRsfWord)funcDef.Word().ElementAt(0).GetText() : null
					};
					RawRsfWord funcVisibility = (RawRsfWord)"private";
					if (funcDef.PublicKeyWord() != null)
					{
						funcVisibility = (RawRsfWord)funcDef.PublicKeyWord().GetText();
					}
					else if (funcDef.PrivateKeyWord() != null)
					{
						funcVisibility = (RawRsfWord)(funcDef.PrivateKeyWord().GetText());
					}
					else if (funcDef.InternalKeyWord() != null)
					{
						funcVisibility = (RawRsfWord)(funcDef.InternalKeyWord().GetText());
					}
					funcData.Visibility = funcVisibility;
					var funcArgList = new List<Tuple<RawRsfWord, RawRsfWord>>();
					foreach (var funcArg in funcDef.typeDefs().typeDef())
					{
						funcArgList.Add(new((RawRsfWord)funcArg.Word().ElementAt(0).GetText(), (RawRsfWord)funcArg.Word().ElementAt(1).GetText()));
						// new((RawRsfWord)type, (RawRsfWord)varName)
					}
					funcData.Arguments = funcArgList;
					var funcCmds = new List<IRawRsfCmd>();
					foreach (var cmd in func.segment().segmentBody().cmd())
					{
						if (cmd.funcWord() != null)
						{
							string callFuncName = cmd.funcWord().GetText();
							if (!string.IsNullOrEmpty(callFuncName))
							{
								callFuncName = callFuncName.ToLower();
								IRawRsfCmd? cmdData = null;
								if (callFuncName == "return")
								{
									if (cmd.GetToken(RULE_value, 0) != null)
									{
										cmdData = new RawRsfReturn()
										{
											ReturnValue = (RawRsfWord)cmd.GetToken(RULE_value, 0).GetText()
										};
									}
									else
									{
										Script.Error.Add($"Error [RS0003]: 'return' command must include a return value. (Line {cmd.funcWord().Word().ElementAt(0).Symbol.Line}, Position {cmd.funcWord().Word().ElementAt(0).Symbol.Column + callFuncName.Length + 1})");
									}
								}
								else if (cmd.Equals() != null)
								{
									if (cmd.Word().ElementAt(0) == null)
									{
										Script.Error.Add($"Error [RS0004]: 'varSet' command must include two variables or one variable with a value. (Line {cmd.Equals().Symbol.Line}, Position {cmd.Equals().Symbol.Column + 1})");
									}
									else if (cmd.Word().Length == 1 && cmd.plainString() == null)
									{
										Script.Error.Add($"Error [RS0004]: 'varSet' command must include two variables or one variable with a value. (Line {cmd.Equals().Symbol.Line}, Position {cmd.Equals().Symbol.Column + 1})");
									}
									else
									{
										if (cmd.plainString() != null)
										{
											cmdData = new RawRsfVariableSet()
											{
												TargetVariable = (RawRsfWord)cmd.Word().ElementAt(0).GetText(),
												FromVariable = null,
												FromValue = (RawRsfPlainString)cmd.plainString().GetText()
											};
										}
										else
										{
											cmdData = new RawRsfVariableSet()
											{
												TargetVariable = (RawRsfWord)cmd.Word().ElementAt(0).GetText(),
												FromVariable = (RawRsfWord)cmd.Word().ElementAt(1).GetText(),
												FromValue = null
											};
										}
									}
								}
								else if (cmd.values() != null)
								{
									var funcCallArguments = new List<IRawRsfValue>();
									var funcCallNames = new List<RawRsfWord>();
									foreach (var value in cmd.values().value())
									{
										if ((value.GetText().StartsWith('\'') && value.GetText().EndsWith('\''))
											|| (value.GetText().StartsWith('"') && value.GetText().EndsWith('"')))
										{
											funcCallArguments.Add((RawRsfPlainString)value.GetText());
										}
										else
										{
											funcCallArguments.Add((RawRsfWord)value.GetText());
										}
									}
									foreach (var funcName in cmd.funcWord().Word())
									{
										funcCallNames.Add((RawRsfWord)funcName.GetText());
									}
									cmdData = new RawRsfFunctionCall()
									{
										FunctionNames = funcCallNames,
										Arguments = funcCallArguments
									};
								}
								else
								{
									Script.Warning.Add($"Warning [RS1005]: Unknown command type. (Line {cmd.funcWord().Word(0).Symbol.Line}, Position {cmd.funcWord().Word(0).Symbol.Column + 1})");
								}
								if (cmdData != null)
								{
									funcCmds.Add(cmdData);
								}
							}
							else
							{
								Script.Error.Add($"Error [RS0002]: Invalid function name of the command. (Line {cmd.GetToken(RULE_funcWord, 0).Symbol.Line}, Position 1)");
							}
						}
						else
						{
							Script.Error.Add($"Error [RS0002]: Invalid function name of the command. (Line {cmd.GetToken(RULE_funcWord, 0).Symbol.Line}, Position 1)");
						}
					}
					funcData.Content = funcCmds;
					funcList.Add(funcData);
				}
				Script.Classes.Add(classData);
			}
		}
	}

	public class RsfErrorListener<T> : IAntlrErrorListener<T>
	{
		private RawRsfScript Script;
		private int LineOffset;

		public RsfErrorListener(RawRsfScript script, int lineOffset = 0)
		{
			LineOffset = lineOffset;
			Script = script;
		}

		public void SyntaxError(IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
			Script.Error.Add($"Error [RS0001]: Syntax Error (Line {line}, Position {charPositionInLine}):\n" + msg);
		}
	}

	public partial class RabotoraScriptFormatParser : Parser
	{
		public partial class ValueContext : ParserRuleContext
		{
			public IRawRsfValue Value;
		}

		public partial class CmdContext : ParserRuleContext
		{
			public IRawRsfValue Value;
		}
	}
}
