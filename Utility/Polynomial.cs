using QRay.Components;
using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public class Polynomial
    {
        public int[] Coefficients { get; set; }
        public int Degree { get; set; }
        public PolynomialNotation Notation { get; set; }
        public int[] CoefficientsReversed
        {
            get
            {
                return Coefficients.Reverse().ToArray();
            }
        }
        public int LeadTerm 
        { 
            get
            {
                return Coefficients[^1];
            }
        }

        public int LeadTermAlpha
        {
            get
            {
                int coeff = Coefficients[^1];
                return coeff == 0 ? 0 : GF256.Log[coeff];
            }
        }

        private static readonly GF256 GF256 = new GF256();

        /// <summary>
        /// Initializes a new instance of the <see cref="Polynomial"/> class with the specified degree and notation.
        /// </summary>
        /// <param name="degree">
        /// The degree of the polynomial, which determines the highest power of x in the polynomial.
        /// </param>
        /// <param name="notation">
        /// The notation type of the polynomial, either <see cref="PolynomialNotation.Alpha"/> or <see cref="PolynomialNotation.Integer"/>.
        /// </param>
        public Polynomial(int degree, PolynomialNotation notation)
        {
            Coefficients = new int[degree + 1];
            Degree = degree;
            Notation = notation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polynomial"/> class with the specified coefficients and notation.
        /// </summary>
        /// <param name="coefficients">
        /// An array of integers representing the coefficients of the polynomial. The coefficients are stored internally in reverse order (so x^0 + x^1 + x^2 ...).
        /// </param>
        /// <param name="notation">
        /// The notation type of the polynomial, either <see cref="PolynomialNotation.Alpha"/> or <see cref="PolynomialNotation.Integer"/>.
        /// </param>
        public Polynomial(int[] coefficients, PolynomialNotation notation, PolynomialOptions options = PolynomialOptions.InitAscending)
        {
            int[] formatted = coefficients;
            if (options == PolynomialOptions.InitDescending)
            {
                formatted = formatted.Reverse().ToArray();
            }
            Coefficients = formatted;
            Degree = coefficients.Length - 1;
            Notation = notation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polynomial"/> class with the specified coefficients and notation.
        /// </summary>
        /// <param name="coefficients">
        /// An array of integers representing the coefficients of the polynomial. The coefficients are stored internally in reverse order (so x^0 + x^1 + x^2 ...).
        /// </param>
        /// <param name="notation">
        /// The notation type of the polynomial, either <see cref="PolynomialNotation.Alpha"/> or <see cref="PolynomialNotation.Integer"/>.
        /// </param>
        public Polynomial(List<int> coefficients, PolynomialNotation notation, PolynomialOptions options = PolynomialOptions.InitAscending)
        {
            int[] formatted = coefficients.ToArray();
            if (options == PolynomialOptions.InitDescending)
            {
                formatted = formatted.Reverse().ToArray();
            }
            Coefficients = formatted;
            Degree = coefficients.Count - 1;
            Notation = notation;
        }

        public Polynomial Clone()
        {
            // Copy coefficients to new array
            int[] coeffs = new int[Coefficients.Length];
            Array.Copy(Coefficients, coeffs, coeffs.Length);

            // Initialize
            return new Polynomial(coeffs, Notation);
        }


        /// <summary>
        /// Removes the leading zero coefficient from the polynomial, if present.
        /// </summary>
        /// <returns>
        /// A new <see cref="Polynomial"/> instance with the leading zero removed, or the current instance if no leading zero exists.
        /// </returns>
        /// <remarks>
        /// This method ensures that the polynomial representation is minimal by discarding unnecessary leading zeros.
        /// The operation is performed in the <see cref="PolynomialNotation.Integer"/> notation for consistency.
        /// </remarks>
        public Polynomial DiscardLeadZero()
        {
            Polynomial poly = ConvertNotation(this, PolynomialNotation.Integer);
            if (Coefficients.Length == 1 || poly.Coefficients[^1] != 0)
            {
                return Clone();
            }

            return new Polynomial(Coefficients[..^1], Notation);
        }


        /// <summary>
        /// Multiplies two polynomials in the Galois Field (GF(256)) using alpha notation.
        /// </summary>
        /// <param name="left">The first polynomial operand.</param>
        /// <param name="right">The second polynomial operand.</param>
        /// <returns>
        /// A new <see cref="Polynomial"/> instance representing the product of the two input polynomials.
        /// The result is in <see cref="PolynomialNotation.Alpha"/> notation.
        /// </returns>
        /// <remarks>
        /// This method ensures that both input polynomials are converted to alpha notation before performing the multiplication.
        /// The multiplication is performed by adding the exponents of the coefficients modulo 255, as per GF(256) rules.
        /// </remarks>
        public static Polynomial operator *(Polynomial left, Polynomial right)
        {
            // Ensuring that we are working with alpha notation
            left = ConvertNotation(left, PolynomialNotation.Alpha);
            right = ConvertNotation(right, PolynomialNotation.Alpha);

            Polynomial product = new Polynomial(left.Degree + right.Degree, PolynomialNotation.Alpha);

            List<int> visitedIndicies = new List<int>();

            for (int i = 0; i <= left.Degree; i++)
            {
                int a = left.Coefficients[i];

                for (int j = 0; j <= right.Degree; j++)
                {
                    int b = right.Coefficients[j];

                    int index = i + j;

                    //int exponent = exponentSum % 256 + exponentSum / 256;
                    //  ^ QRCoder uses this - pointless because exponents cannot be bigger than 508
                    int exponent = (a + b) % 255;

                    if (visitedIndicies.Contains(index))
                    {
                        int currentCoefficient = product.Coefficients[index];
                        int sum = GF256.AntiLog[currentCoefficient] ^ GF256.AntiLog[exponent];
                        product.Coefficients[index] = GF256.Log[sum];
                    }
                    else
                    {
                        product.Coefficients[index] = exponent;
                        visitedIndicies.Add(index);
                    }
                }
            }

            return product;
        }

        public static Polynomial operator ^(Polynomial left, Polynomial right)
        {
            int maxDegree = Math.Max(left.Degree, right.Degree);
            Polynomial result = new Polynomial(maxDegree, left.Notation);

            int leftOffset = maxDegree - left.Degree;
            int rightOffset = maxDegree - right.Degree;

            // Coefficients are in ascending order, so we need to flip it.
            for (int i = 0; i <= maxDegree; i++)
            {
                int leftCoeff = i < leftOffset ? 0 : left.Coefficients[i - leftOffset];
                int rightCoeff = i < rightOffset ? 0 : right.Coefficients[i - rightOffset];
                result.Coefficients[i] = leftCoeff ^ rightCoeff;
            }

            return result;
        }

        public static Polynomial operator *(Polynomial left, int alpha)
        {
            Polynomial result = ConvertNotation(left, PolynomialNotation.Alpha);

            for (int i = 0; i <= left.Degree; i++)
            {
                int sum = result.Coefficients[i] + alpha;
                result.Coefficients[i] = sum % 255;
            }

            return result;
        }

        /// <summary>
        /// Performs polynomial division over Galois Field GF(256) to calculate the Reed-Solomon error correction remainder.
        /// </summary>
        /// <param name="message">
        /// The message polynomial in integer notation.
        /// </param>
        /// <param name="generator">
        /// The generator polynomial in alpha notation (typically built from (x - αᶦ) for i in [0..n-1]).
        /// </param>
        /// <returns>
        /// The remainder polynomial of the division, which contains the Reed-Solomon error correction codewords.
        /// </returns>
        /// <remarks>
        /// This method performs division in GF(256), where:
        /// - Polynomial coefficients are either in integer or alpha notation.
        /// - All math follows GF rules (XOR for subtraction, mod 255 for exponent arithmetic).
        /// 
        /// Leading zero terms are automatically discarded via <see cref="DiscardLeadZero"/> before each reduction.
        /// All intermediate generator multiplications are performed in alpha notation, then converted back to integer before XOR.
        /// </remarks>

        public static Polynomial operator /(Polynomial message, Polynomial generator)
        {
            Polynomial result = message.Clone();

            // Polynomial division in Galois Field (256)
            for (int i = 0; i < message.Degree + 1; i++)
            {
                // Step 1: Getting the lead term of the message polynomial - alpha notation
                int leadTerm = result.LeadTermAlpha;

                if (result.LeadTerm == 0)
                {
                    result = result.DiscardLeadZero();
                    continue;
                }

                // Step 2: Multiply the generator polynomial with the lead term, and convert to integer notation
                Polynomial product = ConvertNotation(generator * leadTerm, PolynomialNotation.Integer);

                // Step 3: XOR the message polynomial with the result from step 2
                product = result ^ product;
                result = product.DiscardLeadZero();
            }

            return result;
        }

        /// <summary>
        /// Converts the notation of a polynomial between Alpha and Integer representations.
        /// </summary>
        /// <param name="p">
        /// The <see cref="Polynomial"/> instance whose notation is to be converted.
        /// </param>
        /// <param name="targetNotation">
        /// The target <see cref="PolynomialNotation"/> to which the polynomial should be converted.
        /// </param>
        /// <returns>
        /// A new <see cref="Polynomial"/> instance with the converted notation, or the original instance if no conversion is needed.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the target notation type does not exist or is invalid.
        /// </exception>
        /// <remarks>
        /// - When converting from Alpha to Integer notation, the coefficients are replaced with their corresponding values from the GF(256) AntiLog table.
        /// - When converting from Integer to Alpha notation, the coefficients are replaced with their corresponding values from the GF(256) Log table.
        /// - If the polynomial is already in the target notation, the original instance is returned.
        /// </remarks>
        public static Polynomial ConvertNotation(Polynomial p, PolynomialNotation targetNotation)
        {
            // Original notation switch
            switch (p.Notation)
            {
                case PolynomialNotation.Alpha:

                    // Target notation switch
                    switch (targetNotation)
                    {
                        case PolynomialNotation.Alpha:
                            return p.Clone();
                        case PolynomialNotation.Integer:
                            Polynomial result = new Polynomial(p.Degree, targetNotation);

                            // Convert each coefficient to integer notation  
                            for (int i = 0; i < p.Degree + 1; i++)
                            {
                                result.Coefficients[i] = GF256.AntiLog[p.Coefficients[i]];
                            }

                            return result;
                        default:
                            throw new ArgumentException("Target notation type does not exist.");
                    }
                case PolynomialNotation.Integer:

                    // Target notation switch
                    switch (targetNotation)
                    {
                        case PolynomialNotation.Integer:
                            return p.Clone();
                        case PolynomialNotation.Alpha:
                            Polynomial result = new Polynomial(p.Degree, targetNotation);

                            // Convert each coefficient to alpha notation  
                            for (int i = 0; i < p.Degree + 1; i++)
                            {
                                result.Coefficients[i] = GF256.Log[p.Coefficients[i]];
                            }

                            return result;
                        default:
                            throw new ArgumentException("Target notation type does not exist.");
                    }
                default:
                    throw new ArgumentException("Notation type does not exist");
            }
        }

        /// <summary>
        /// Converts the polynomial to its string representation.
        /// </summary>
        /// <returns>
        /// A string representation of the polynomial in the specified notation.
        /// </returns>
        /// <remarks>
        /// - If the polynomial is in <see cref="PolynomialNotation.Alpha"/> notation, coefficients are represented as powers of alpha (e.g., "a^2").
        /// - If the polynomial is in <see cref="PolynomialNotation.Integer"/> notation, coefficients are represented as integers.
        /// - Terms are ordered from the highest degree to the lowest degree.
        /// - The power of x is appended to each term, except for the constant term (x^0).
        /// </remarks>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = Degree; i >= 0; i--)
            {
                int coeff = Coefficients[i];

                // If it's not the first term, add the plus sign
                if (sb.Length > 0 && coeff >= 0)
                {
                    sb.Append(" + ");
                }

                if (coeff < 0)
                {
                    sb.Append(" - ");
                    coeff = -coeff; // Make it positive for the next part
                }

                if (Notation == PolynomialNotation.Alpha)
                {
                    // Use alpha notation (𝛼^coeff) for the coefficient part
                    if (coeff == 1 && i > 0)
                    {
                        sb.Append("a");
                    }
                    else
                    {
                        sb.Append("a^" + coeff);
                    }
                }
                else if (Notation == PolynomialNotation.Integer)
                {
                    sb.Append(coeff);
                }

                // Handle the power of x (if i > 0, we are dealing with x terms)
                if (i > 0)
                {
                    sb.Append("x");

                    // If power is not 1, add the exponent
                    if (i > 1)
                    {
                        sb.Append("^" + i);
                    }
                }
            }

            return sb.ToString();
        }


    }
}
