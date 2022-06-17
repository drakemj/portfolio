using System;
using System.Collections.Generic;
using System.Linq;
/*
Hey what's going on youtube, it's ya boy Roddy Rickle and I bring you christmas tidings and a 
sick random generation script.

MAIN IDEA:
create a script that creates a three dimensional array (called the data array). The two dimensions are used 
to index the coordinates of our 2D game, and the third holds data about each room. The first element of the 
array determines the type of room, and the current other four (i believe i will need to add more for the
decreasing probability for adding smaller dead end branches) tell which side of the cube rooms have
openings, closing, or is unloaded. Since c# defaults unspecified array elements to zero, I decided
to make this the unloaded symbol for all purposes.

KEY POINT GENERATION:
The program creates a Map class with a generation function that generates everything. It starts off
by generating a random start and end room (which are checked for several conditions, including how
close they are to each other) as well as three midways/ checkpoints. These are indicated by the 
numbers 2, 3, and 4 respectively in the first element of the z element of the data array.
The midways are generated from an orientation on the x or y axis randomly, which means if it is oriented
on the x axis, the x coordinates of the midpoints will be equally spaced in between the x coordinates of the
start and end room's x coordinates. Whether a certain generation is x or y oriented is completely random and
generated when the main function calls generate(). The other axis that it is not oriented to has just random
coordinates. This creates a path that does not loop back on itself (though if you wanted to, you could create
another midpoint to change this with careful parameters.)

MAIN PATH GENERATION:
These values are updated in the data array, and then the closest path in between them is executed by iterating
through, calculating which of the four directions is the closest to the next midpoint. The beauty of this design 
is that because of the way the midpoints are set up, there ofc will be no possible way to have the closest path
be the room you just left from or a room that is non indexable or out of bounds. The rooms are marked as loaded
in the data array and as each room is loaded in, the data array marks the wall facing the room it just left 
from as open. The other walls are left unloaded to be generated later, in case. This ensures that there is
always a path to the next room, and therefore always a path through the whole thing.

BRANCH PATH GENERATION PLANS:
The next step, which has yet to be finished, has to do with generating random offshoots from the main path. 
The idea is to have most of the path's walls be closed (note to deal with paths that double back on each other
on midways) while some randomly are loaded as open, which will generate "dead end" branches. To make exploring
these as least tedious as possible, the probability that subsequent rooms will have openings and thus more 
branches will decrease the further it gets from the main path (something i could track with that sixth element
in the data array that i mentioned earlier.) The generation of the whole array will finish when every current
room has finished loading each element in its z array (note: a recursive function may be useful in this 
problem, havent thought it through too much yet.)

Feel free to make your own changes and be sure to let me know.


*/
//creating map class
class Map
{
    public int[,,] data = new int[20, 20, 6];      // Create a three dimensional array that
    Random rnd = new Random();                       // holds values for openings of a square room

    struct Coordinates
    {                              // struct for coordinates
        public int x;
        public int y;

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public void generate()
    {
        Coordinates start = new Coordinates();     // make start the coords for beginning room

        do
        {
            start.x = rnd.Next(9);                   // Make two random numbers for starting room
            start.y = rnd.Next(9);
        } while (start.x + start.y > 12);        // Make sure starting point is somewhat near bottom and left edges

        data[start.x, start.y, 0] = 2;                        // value in z element is start/end room

        Coordinates final = new Coordinates();                // final is the exit room coords
        float diff;
        int deltaX;
        int deltaY;
        do
        {
            final.x = rnd.Next(20);
            final.y = rnd.Next(20);
            diff = Diff(final.x, final.y, start.x, start.y);
            deltaX = Math.Abs(final.x - start.x);
            deltaY = Math.Abs(final.y - start.y);
        } while (diff < 13.0f || deltaX < 4 || deltaY < 4);         // make sure end room is far enough away from starting room, and that it has some distance away on the x and y axes both to ensure the path isn't just a straight line

        data[final.x, final.y, 0] = 3;                        // set this to end room point

        int xSplit = (final.x - start.x) / 4;        // set possible x and y coordinates, may not be used
        int ySplit = (final.y - start.y) / 4;        // depending on orientation
        int orient = rnd.Next(2);                  // randomly set orientation to x or y axis

        Coordinates point1 = new Coordinates();     // declare three midpoint coordinates
        Coordinates point2 = new Coordinates();
        Coordinates point3 = new Coordinates();     // create vars for midway coords
        Coordinates empty = new Coordinates();
        Coordinates[] midpoints = { point1, point2, point3, empty };  //create midpoint array

        if (orient == 1)
        {                                 //depending on orientation, set points
            for (int i = 0; i < 3; i++)
            {
                midpoints[i].x = xSplit * (i + 1) + start.x;      // if oriented on x axis, each mid point
                do
                {                                           // x coord is xSplit away from each other
                    midpoints[i].y = rnd.Next(20);
                } while (3.0f > Diff(midpoints[i].x, midpoints[i].y, final.x, final.y) && 3.0f > Diff(midpoints[i].x, midpoints[i].y, start.x, start.y));
            } // above, the y coordinates are mostly random when x oriented, but just made sure they are
        }   // not too short of a distance from the start and end rooms
        else
        {
            for (int i = 0; i < 3; i++)
            {                    // identical to the x orientation but with y
                midpoints[i].y = ySplit * (i + 1) + start.y;
                do
                {
                    midpoints[i].x = rnd.Next(20);
                } while (3.0f > Diff(midpoints[i].x, midpoints[i].y, final.x, final.y) && 3.0f > Diff(midpoints[i].x, midpoints[i].y, start.x, start.y));
            }
        }

        for (int i = 0; i < 3; i++)
        {
            data[midpoints[i].x, midpoints[i].y, 0] = 4;   // setting the value to 4 in the 0th elment of the z array
        }

        // at this point we have five points, a start, finish, and three midpoints.
        // order of array elements to holes: right, up, left, down (array elements 1-4)
        // open, closed = 1, 2


        Coordinates currentGen = new Coordinates(start.x, start.y);  // keep track of current coords
        midpoints[3] = final;                                         // in this var
        int direction = 0;

        /*above, the final coordinates are put into the final element of the midpoints array to
          allow the the room generation to iterate through it (it's treated like another midpoint)


          below, the generation for pathing starts. loaded rooms are indicated by 1 in the first
          element of the z array.
        */
        for (int i = 0; i < 4; ++i)
        {
            for (; currentGen.x != midpoints[i].x || currentGen.y != midpoints[i].y;)
            {
                direction = DirectionGen(currentGen, midpoints[i]);     //function that determines the closest path to the next checkpoint
                switch (direction)
                {
                    case 1:
                        if (data[currentGen.x + 1, currentGen.y, 0] == 0)
                        { //check if unloaded          
                            data[currentGen.x + 1, currentGen.y, 0] = 1;       //set room to loaded
                        }
                        data[currentGen.x, currentGen.y, 1] = 1;           //set opening to previous room
                        data[currentGen.x + 1, currentGen.y, 3] = 1;       //set opening to current room
                        currentGen.x += 1;                                 // iterate currentGen to loaded room
                        break;
                    case 2:                                               // rinse and repeat
                        if (data[currentGen.x, currentGen.y + 1, 0] == 0)
                        {
                            data[currentGen.x, currentGen.y + 1, 0] = 1;
                        }
                        data[currentGen.x, currentGen.y, 2] = 1;
                        data[currentGen.x, currentGen.y + 1, 4] = 1;
                        currentGen.y += 1;
                        break;
                    case 3:
                        if (data[currentGen.x - 1, currentGen.y, 0] == 0)
                        {
                            data[currentGen.x - 1, currentGen.y, 0] = 1;
                        }
                        data[currentGen.x, currentGen.y, 3] = 1;
                        data[currentGen.x - 1, currentGen.y, 1] = 1;
                        currentGen.x -= 1;
                        break;
                    case 4:
                        if (data[currentGen.x, currentGen.y - 1, 0] == 0)
                        {
                            data[currentGen.x, currentGen.y - 1, 0] = 1;
                        }
                        data[currentGen.x, currentGen.y, 4] = 1;
                        data[currentGen.x, currentGen.y - 1, 2] = 1;
                        currentGen.y -= 1;
                        break;
                    default:
                        break;
                }

            }
        }

    }

    float Diff(int x1, int y1, int x2, int y2)
    {      //difference method for two points on a plane
        float difference = (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        return difference;
    }

    int DirectionGen(Coordinates p1, Coordinates p2)
    {
        float diffRight;                                // set floats to hold distance from each
        float diffUp;                                   // the adjacent coordinates from p1
        float diffLeft;                                 // to p2
        float diffDown;

        diffRight = Diff(p1.x + 1, p1.y, p2.x, p2.y);
        diffUp = Diff(p1.x, p1.y + 1, p2.x, p2.y);
        diffLeft = Diff(p1.x - 1, p1.y, p2.x, p2.y);
        diffDown = Diff(p1.x, p1.y - 1, p2.x, p2.y);
        float[] vals = { diffRight, diffUp, diffLeft, diffDown };  // create array to find min value
        float min = vals.Min();

        List<int> tie = new List<int>();               // in the case of a tie, randomly pick from the 2
        for (int i = 0; i < 4; i++)
        {
            if ((int)vals[i] == (int)min)
            {
                tie.Add(i);
            }
        }
        if (tie.Count == 2)
        {
            int randIndex = rnd.Next(2);
            return tie[randIndex] + 1;                   // the array is sorted so that this index will
        }                                              // magically line up with the switch :)
        else
        {
            return (Array.IndexOf(vals, min) + 1);
        }
    }
    
    int NextCount(Coordinates currentPos, int[, ,] data)
    {
        int total = 0;
        if (data[currentPos.x + 1, currentPos.y, 0] != 0)
            total += 1;
        if (data[currentPos.x, currentPos.y + 1, 0] != 0)
            total += 1;
        if (data[currentPos.x - 1, currentPos.y, 0] != 0)
            total += 1;
        if (data[currentPos.x, currentPos.y - 1, 0] != 0)
            total += 1;
        return total;
    }
    
    bool CurrentlyLoaded(Coordinates currentPos, int[,,] data)
    {
        return data[currentPos.x, currentPos.y, 0] != 0;
    }
}


class MainClass
{
    public static void Main(string[] args)
    {
        Map test = new Map();
        test.generate();
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                Console.Write(test.data[i, j, 0]);
                Console.Write(" ");
            }
            Console.Write("\n");
        }

    }
}