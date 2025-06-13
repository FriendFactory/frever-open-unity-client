namespace Tests.EditMode
{
    public class RandomIdGenerator: IIdGenerator
    {
        private readonly System.Random _generator;

        public RandomIdGenerator(int seed = 0)
        {
            _generator = new System.Random(seed);
        }
        
        public long Next()
        {
            return _generator.Next();
        }
    }
}