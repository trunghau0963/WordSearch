// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("j3UbYARMiCK5VW3UbpbwInTqSVo2hAckNgsADyyAToDxCwcHBwMGBXAyWsQpv76yPys4z4jOjwqpVdBRFxaCLAIY+EzBWD5Lp7oE+gPFRx1c7payGbylF8MQ8YLxDvuoRFV2WVfDJWgGeBWJ/EKW6t+OR0rX5ogXF5qYegbEnUK2JbZgxmy38FzU+xKUTblvpfXAsHFg4DYx+dILA938MD49WlRDcpU1PDWlUF0G77o4Gq5x07XX+UzpQ5+Uy9e0CgOqdaYX2CTx6HeDDkyspsJgM3KUSJlnijELSoQHCQY2hAcMBIQHBwa1HutWtnkEg/OrernkcBhxbbtYNxxGo4u0kI6267ojChcMtz9zhysCxU6QoF5TwPyYJE41NKBh2wQFBwYH");
        private static int[] order = new int[] { 11,11,13,7,5,6,9,7,8,9,12,11,12,13,14 };
        private static int key = 6;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
