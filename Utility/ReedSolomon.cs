using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public class ReedSolomon
    {
        private Polynomial Generator;
        private int ECCPerBlock = 0;
        public List<List<List<int>>> ApplyErrorCorrection(List<List<List<int>>> groups)
        {
            List<List<List<int>>> result = new List<List<List<int>>>();

            for (int g = 0; g < groups.Count; g++)
            {
                result.Add(new List<List<int>>());

                for (int b = 0; b < groups[g].Count; b++)
                {
                    result[g].Add(Apply(groups[g][b]));
                }
            }

            return result;
        }

        public List<int> Apply(List<int> message)
        {
            Polynomial messagePoly = new Polynomial(message, PolynomialNotation.Integer, PolynomialOptions.InitDescending);

            Polynomial result = messagePoly / Generator;

            List<int> coeffs = result.CoefficientsReversed.ToList();

            // Add trailing zeros if necessary
            if (result.Coefficients.Length < ECCPerBlock)
            {
                int missingCoeffs = ECCPerBlock - result.Coefficients.Length;
                coeffs.AddRange(Enumerable.Repeat(0, missingCoeffs));
            }

            return coeffs;
        }

        public ReedSolomon(int eccPerBlock)
        {
            ECCPerBlock = eccPerBlock;
            Generator = GeneratorPolynomial.Get(eccPerBlock, PolynomialNotation.Alpha);
        }
    }
}
