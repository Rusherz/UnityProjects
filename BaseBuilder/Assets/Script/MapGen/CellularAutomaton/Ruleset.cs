using System.Collections.Generic;

namespace ProceduralToolkit.Examples
{
    /// <summary>
    /// Cellular automaton ruleset representation
    /// </summary>
    public struct Ruleset
    {
        #region Common rulesets

        public static Ruleset majority { get { return new Ruleset("5678", "45678"); } }

        #endregion Common rulesets

        private readonly List<int> birthRule;
        private readonly List<int> survivalRule;

        public Ruleset(string birthRule = null, string survivalRule = null)
        {
            this.birthRule = ConvertRuleStringToList(birthRule);
            this.survivalRule = ConvertRuleStringToList(survivalRule);
        }

        public bool CanSpawn(int aliveCells)
        {
            return birthRule.Contains(aliveCells);
        }

        public bool CanSurvive(int aliveCells)
        {
            return survivalRule.Contains(aliveCells);
        }

        private static List<int> ConvertRuleStringToList(string rule)
        {
            var list = new List<int>();
            if (!string.IsNullOrEmpty(rule))
            {
                foreach (char c in rule)
                {
                    if (char.IsDigit(c))
                    {
                        list.Add((int) char.GetNumericValue(c));
                    }
                }
                list.Sort();
            }
            return list;
        }

        public override string ToString()
        {
            string b = "";
            foreach (var digit in birthRule)
            {
                b += digit;
            }
            string s = "";
            foreach (var digit in survivalRule)
            {
                s += digit;
            }
            return string.Format("B{0}/S{1}", b, s);
        }
    }
}