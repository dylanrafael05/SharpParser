namespace Testing
{
    // Define a class that describes a person with an age, name, and height
    public class Person 
    {
        public int Age { get; set; }
        public string Name { get; set;}
        public float Height { get; set; }

        // Construct a new instance
        public Person(int age, string name, float height)
        {
            Age = age;
            Name = name;
            Height = height;
        }
    }
}