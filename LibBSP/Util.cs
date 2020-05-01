namespace LibBSP
{
    public static class Util
    {
        /// <summary>
        /// Rounds a number up to nearest multiple
        /// </summary>
        /// <param name="numToRound">Number to round</param>
        /// <param name="multiple">The multiple to round up to</param>
        /// <returns>The rounded number</returns>
        public static int RoundUp(int numToRound, int multiple)
        {
            var remainder = numToRound % multiple;
            if (remainder == 0)
                return numToRound;

            return numToRound + multiple - remainder;
        }
    }
}