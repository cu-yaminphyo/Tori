using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMTORI
{
    public class OwnBank
    {
        public string OwnId { get; set; }
        public string FacId { get; set; }
        public string Bank { get; set; }
        public string Branch { get; set; }
        public string Kind { get; set; }
        public string Account { get; set; }
        public string ContractNo { get; set; }
        public string Factoring { get; set; }
        public string Item { get; set; }

        public OwnBank()
        {
            this.OwnId = string.Empty;
            this.FacId = string.Empty;
            this.Bank = string.Empty;
            this.Branch = string.Empty;
            this.Kind = string.Empty;
            this.Account = string.Empty;
            this.ContractNo = string.Empty;
            this.Factoring = string.Empty;
            this.Item = string.Empty;
        }

        public override string ToString()
        {
            return this.Item;
        }

    }
}
