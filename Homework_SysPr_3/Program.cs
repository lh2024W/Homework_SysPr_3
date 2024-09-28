
namespace Homework_SysPr_3
{
    enum Role { Forager, Builder, Caretaker, Qeen }
    public class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;
            Console.Clear();

            AntColonySimulation simulation = new AntColonySimulation(25, 10, 10);

            Console.SetCursorPosition(0, 11);
            Console.WriteLine("Running an anthill simulation:");
            Console.SetCursorPosition(0, 0);
            simulation.StartSimulation();
            Console.WriteLine("Simulation complete.");
            Console.CursorVisible = true;
        }
    }

    class Ant
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Energy { get; set; }
        public int Age { get; set; }

        public Role AntRole { get; set; }
        private Random random = new Random();
        private AntColonySimulation simulation;


        public Ant(int x, int y, Role role, AntColonySimulation simulation)
        {
            X = x;
            Y = y;
            AntRole = role;
            Energy = 100;
            Age = 0;
            this.simulation = simulation;
        }

        public void Move()
        {
            X = (X + random.Next(-1, 2)) % simulation.Width;
            Y = (Y + random.Next(-1, 2)) % simulation.Height;

            if (X < 0) X = simulation.Width - 1;
            if (Y < 0) Y = simulation.Height - 1;
        }

        public void PerformTask()
        {
            switch (AntRole)
            {
                case Role.Forager:
                    SearchFood();
                    break;
                case Role.Builder:
                    Build();
                    break;
                case Role.Caretaker:
                    TakeCareOfQeen();
                    break;
                case Role.Qeen:
                    Rest();
                    break;
            }
        }

        private void SearchFood()
        {
            Move();
            Energy -= 10;
        }

        private void Build()
        {
            Move();
            Energy -= 5;
        }

        private void TakeCareOfQeen()
        {
            Energy -= 2;
        }

        private void Rest()
        {
            Energy += 5;
        }

        public void Work()
        {
            while (Energy > 0)
            {
                PerformTask();
                simulation.UpdateMap();
                simulation.UpdateInfo(this);
                Thread.Sleep(500);
            }
        }
    }

    class AntColonySimulation
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private char[,] world;
        private List<Ant> ants;
        private List<Thread> antThreads;
        private int infoStartY;


        public AntColonySimulation(int width, int height, int numberOfAnts)
        {
            Width = width;
            Height = height;
            world = new char[width, height];
            ants = new List<Ant>();
            antThreads = new List<Thread>();

            infoStartY = height + 2;

            Random random = new Random();
            for (int i = 0; i < numberOfAnts; i++)
            {
                Role role = (Role)random.Next(0, 4);
                var ant = new Ant(random.Next(0, width), random.Next(0, height), role, this);
                ants.Add(ant);

                Thread thread = new Thread(ant.Work);
                antThreads.Add(thread);
            }
        }

        public void UpdateMap()
        {
            lock (world)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        world[i, j] = '.';
                    }
                }
                foreach (var ant in ants)
                {
                    world[ant.X, ant.Y] = 'A';
                }

                DrawMap();
            }
        }

        public void DrawMap()
        {
            Console.SetCursorPosition(0, 0);
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    Console.Write(world[i, j]);
                }
                Console.WriteLine();
            }
        }

        public void UpdateInfo(Ant ant)
        {
            lock (world)//Синхронизация потоков
            {
                int infoYPosition = infoStartY + ants.IndexOf(ant);
                Console.SetCursorPosition(0, infoYPosition);
                Console.WriteLine($"Ant {ants.IndexOf(ant) + 1}: Role: {ant.AntRole}, Energy: {ant.Energy}, Age: {ant.Age}      ");
            }
        }

        public void StartSimulation ()
        {
            foreach (var thread in antThreads)
            {
                thread.Start();
            }

            foreach (var thread in antThreads)
            {
                thread.Join();
            }
        }
    }
}
