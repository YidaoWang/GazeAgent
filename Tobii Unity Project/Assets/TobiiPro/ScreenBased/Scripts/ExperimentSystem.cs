namespace Assets.TobiiPro.ScreenBased.Scripts
{
    public class ExperimentSystem
    {
        public int[] ExperimentOrder { get; set; }
        public int RepeatNumber { get; set; }

        public ExperimentSystem(string experimentOrder, string repeatNumber)
        {
            ExperimentOrder = new int[6];
            for(int i = 0; i < 6; i++)
            {
                ExperimentOrder[i] = experimentOrder[i] - '0';
            }
            RepeatNumber = int.Parse(repeatNumber);
        }  
    }
}
