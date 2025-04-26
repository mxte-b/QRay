using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public static class GeneratorPolynomial
    {
        public static Polynomial Get(int codewords, PolynomialNotation notation = PolynomialNotation.Alpha)
        {
            Polynomial result = new Polynomial(1, PolynomialNotation.Alpha);
            result.Coefficients[0] = 0; // x^0
            result.Coefficients[1] = 0; // x^1

            for (int i = 1; i < codewords; i++)
            {
                var next = new Polynomial(1, PolynomialNotation.Alpha);
                next.Coefficients[0] = i;
                next.Coefficients[1] = 0;

                result = result * next;
            }

            return Polynomial.ConvertNotation(result, notation);
        }
    }
}
