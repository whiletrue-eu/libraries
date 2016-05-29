using System;
using System.Text.RegularExpressions;
using WhileTrue.Classes.Utilities;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.SmartCardChannels
{
    public class AutoResponseTrigger
    {
        private readonly Regex anyAtomicPi = new Regex(@"\[[^\[\]]*\]", RegexOptions.RightToLeft);
        private readonly string dynamicCommandConstructionExpression;
        private readonly Regex ifPi = new Regex(@"\[IF:(?<test1>[0-9A-F]{2})==(?<test2>[0-9A-F]{2}),(?<val1>[0-9A-F]{2}),(?<val2>[0-9A-F]{2})\]");
        private readonly Regex maxPi = new Regex(@"\[MAX:(?<val1>[0-9A-F]{2}),(?<val2>[0-9A-F]{2})\]");
        private readonly Regex minPi = new Regex(@"\[MIN:(?<val1>[0-9A-F]{2}),(?<val2>[0-9A-F]{2})\]");
        private readonly Regex originalCommandTrigger;
        private readonly Regex originalResponseTrigger;
        private readonly Regex wildcardPi = new Regex(@"\[(?<type>[CR]):(?<name>[A-Za-z][A-Za-z0-9]+)\]");
        private Match commandMatch;
        private Match responseMatch;

        public AutoResponseTrigger(string originalCommandTrigger, string originalResponseTrigger, string dynamicCommandConstructionExpression)
            : this(new Regex(originalCommandTrigger), new Regex(originalResponseTrigger), dynamicCommandConstructionExpression)
        {
        }

        public AutoResponseTrigger(Regex originalCommandTrigger, Regex originalResponseTrigger, string dynamicCommandConstructionExpression)
        {
            this.originalCommandTrigger = originalCommandTrigger;
            this.originalResponseTrigger = originalResponseTrigger;
            this.dynamicCommandConstructionExpression = dynamicCommandConstructionExpression.Replace(" ", "");
        }

        internal bool IsMatch(CardCommand command, CardResponse response)
        {
            return this.originalCommandTrigger.IsMatch(Conversion.ToHexString(command.Serialize()))
                   && this.originalResponseTrigger.IsMatch(Conversion.ToHexString(response.Serialize()));
        }

        internal CardCommand GetCommand(CardCommand command, CardResponse response)
        {
            this.commandMatch = this.originalCommandTrigger.Match(Conversion.ToHexString(command.Serialize()));
            this.responseMatch = this.originalResponseTrigger.Match(Conversion.ToHexString(response.Serialize()));

            string DynamicCommand = this.Resolve(this.dynamicCommandConstructionExpression);

            this.commandMatch = null;
            this.responseMatch = null;

            return new CardCommand(Conversion.ToByteArray(DynamicCommand));
        }


        private string Resolve(string expression)
        {
            while (this.anyAtomicPi.IsMatch(expression))
            {
                MatchCollection Matches = this.anyAtomicPi.Matches(expression);
                foreach (Match Match in Matches)
                {
                    string Result = Match.Value;
                    if (this.wildcardPi.IsMatch(Result))
                    {
                        Result = this.ResolveWildcard(this.wildcardPi.Match(Result));
                    }
                    if (this.minPi.IsMatch(Result))
                    {
                        Result = this.ResolveFunctionMin(this.minPi.Match(Result));
                    }
                    if (this.maxPi.IsMatch(Result))
                    {
                        Result = this.ResolveFunctionMax(this.maxPi.Match(Result));
                    }
                    if (this.ifPi.IsMatch(Result))
                    {
                        Result = this.ResolveFunctionIf(this.ifPi.Match(Result));
                    }

                    expression = expression.Remove(Match.Index, Match.Length);
                    expression = expression.Insert(Match.Index, Result);
                }
            }

            return expression;
        }

        private string ResolveWildcard(Match match)
        {
            string Type = match.Groups["type"].Value;
            string Name = match.Groups["name"].Value;

            Match SearchMatch;

            SearchMatch = (Type == "C") ? this.commandMatch : this.responseMatch;

            if (SearchMatch.Groups[Name] != null)
            {
                return SearchMatch.Groups[Name].Value;
            }
            else
            {
                throw new Exception("command APDU could not be generated: group not found");
            }
        }

        private string ResolveFunctionMax(Match match)
        {
            string Val1 = this.Resolve(match.Groups["val1"].Value);
            string Val2 = this.Resolve(match.Groups["val2"].Value);

            if (Convert.ToInt32(Val1, 16) > Convert.ToInt32(Val2, 16))
            {
                return Val1;
            }
            else
            {
                return Val2;
            }
        }

        private string ResolveFunctionMin(Match match)
        {
            string Val1 = this.Resolve(match.Groups["val1"].Value);
            string Val2 = this.Resolve(match.Groups["val2"].Value);

            if (Convert.ToInt32(Val1, 16) < Convert.ToInt32(Val2, 16))
            {
                return Val1;
            }
            else
            {
                return Val2;
            }
        }

        private string ResolveFunctionIf(Match match)
        {
            string Test1 = this.Resolve(match.Groups["test1"].Value);
            string Test2 = this.Resolve(match.Groups["test2"].Value);
            string Val1 = this.Resolve(match.Groups["val1"].Value);
            string Val2 = this.Resolve(match.Groups["val2"].Value);

            if (Test1 == Test2)
            {
                return Val1;
            }
            else
            {
                return Val2;
            }
        }
    }
}