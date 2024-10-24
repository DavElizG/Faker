using Api.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Interfaces.Generators
{
    public interface IAffiliateGeneratorService
    {
        List<Affiliate> GetAffiliates();
        void ForceGenerateAffiliate();
    }
}
