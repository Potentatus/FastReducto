using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastReducto
{
    class Node : IComparable
    {
        public bool Exist;
        public uint R, G, B;
        public int PixelsCount;
        public Node[] Childs;
        public Node Parent;
        public byte Level;
        public Node()
        {
            Exist = true;
            Level = 0;
            PixelsCount = 0;
            Childs = new Node[8];
            Parent = null;
            R = 0;
            G = 0;
            B = 0;
        }
        public Node(int level)
        {
            Exist = true;
            Level = (byte)level;
            PixelsCount = 0;
            Childs = new Node[8];
            Parent = null;
            R = 0;
            G = 0;
            B = 0;
        }

        public int CompareTo(object obj)
        {
            Node tmp = (Node)obj;
            if (PixelsCount < tmp.PixelsCount)
                return -1;
            else if (PixelsCount == tmp.PixelsCount)
                return 0;
            else
                return 1;
        }
    }
    class Octree
    {
        //prosta implementacja drzewa Octree
        //jeden minus - węzły są usuwane i tworzone, zamiast zaznaczane i odznaczane - zbrakło czasu od pomysłu do realizacji
        //pokomentowane obszary kodu dotyczą wersji ze znakowaniem węzłów zamiast usuwania
        //niestety wersja ta nie została ukończona na czas
        public Node Root;
        public List<List<Node>> Levels;
        public int ColorsCount;

        public Octree()
        {
            Root = new Node();
            Levels = new List<List<Node>>();
            for (int i = 0; i < 8; i++)
            {
                Levels.Add(new List<Node>());
            }
            Levels[0].Add(Root);

            ColorsCount = 0;
        }

        public Color TranslateColor(Color color)
        {
            Node tmp = FindNode(color, false);
            return Color.FromArgb((int)(tmp.R / tmp.PixelsCount), (int)(tmp.G / tmp.PixelsCount), (int)(tmp.B / tmp.PixelsCount));
        }

        public void AddAndReductColor(Color c, int colors_count)
        {
            AddColor(c);
            ReductTree(colors_count, true);
        }

        public void AddColor(Color color)
        {
            //wyszukanie właściwego liścia
            Node tmp = FindNode(color, true);
            //wstawienie koloru
            tmp.R += color.R;
            tmp.G += color.G;
            tmp.B += color.B;
            tmp.PixelsCount++;
            //jeśli dodaliśmy właśnie liść (nowy kolor)
            if (tmp.PixelsCount == 1)
                ColorsCount++;
        }

        private int GenerateIndex(Color color, int level)
        {
            int mask = 1, result = 0, tmp;
            //red
            tmp = color.R >> (7 - level);
            tmp = tmp & mask;
            result += tmp << 2;
            //green
            tmp = color.G >> (7 - level);
            tmp = tmp & mask;
            result += tmp << 1;
            //blue
            tmp = color.B >> (7 - level);
            tmp = tmp & mask;
            result += tmp;

            //development
            if (result > 7)
                throw new ArgumentOutOfRangeException();
            return result;
        }

        public void ReductNode(Node parent, bool step)
        {
            ////sprawdzamy, czy węzeł istnieje w drzewie
            //if (!parent.Exist)
            //    return;

            //jeśli parent jest redukowany poraz pierwszy należy dodać go do listy kolorów
            //potem tylko zmieniamy jego wartość :D
            if (parent.PixelsCount == 0)
                ColorsCount++;
            int child_level = parent.Level + 1;
            for (int i = 0; i < 8; i++)
            {
                if (parent.Childs[i] != null && parent.Childs[i].Exist)
                {
                    //przepisanie do parenta
                    parent.R += parent.Childs[i].R;
                    parent.G += parent.Childs[i].G;
                    parent.B += parent.Childs[i].B;
                    parent.PixelsCount += parent.Childs[i].PixelsCount;
                    parent.Childs[i].Parent = null;

                    ////wyczyszczenie childa - przy znakowaniu nodów
                    //parent.Childs[i].Exist = false;
                    //parent.Childs[i].R = 0;
                    //parent.Childs[i].G = 0;
                    //parent.Childs[i].B = 0;
                    //parent.Childs[i].PixelsCount = 0;
                    ColorsCount--;

                    //usuwanie, które spowalnia
                    //pośrednie rozwiązanie - nie usuwamy przy całkowitej redukcji
                    if(step)
                    {
                        if (child_level <= 7)
                            Levels[child_level].Remove(parent.Childs[i]);
                    }
                    parent.Childs[i] = null;

                }
            }
        }

        public Node FindNode(Color color, bool insert)
        {
            //szukanie koloru w drzewie - zwraca Node z najbliższym kolorem (zgodnym bitowo od najstarszego bitu)
            //tworzymy node iterujący
            Node tmp;
            tmp = Root;
            int index, i;

            //iterujemy się przez kolejne poziomy aż do rodzica naszego koloru
            for (i = 0; i < 8; i++)
            {
                //generujemy numer podgałęzi
                index = GenerateIndex(color, i);
                if (tmp.Childs[index] == null)
                {
                    if (insert)
                    {
                        tmp.Childs[index] = new Node(i + 1);
                        tmp.Childs[index].Parent = tmp;
                        if (i < 7) Levels[i + 1].Add(tmp.Childs[index]);
                    }
                    //jeśli szukamy i niżej się nie da - trzeba sprawdzić, czy aby nie jest to fałszywy node
                    else
                    {
                        if(tmp.PixelsCount==0)
                        {
                            while (tmp.PixelsCount == 0)
                                tmp = tmp.Parent;
                        }
                        break;
                    }
                }
                //else if(!tmp.Childs[index].Exist)
                //{
                //    tmp.Childs[index].Exist = true;
                //}
                tmp = tmp.Childs[index];
            }
            return tmp;
        }

        public void ReductTree(int colors_count, bool step = false)
        {
            if (ColorsCount <= colors_count)
                return;
            //redukujemy drzewo do zadanej liczby kolorów
            //iterujemy się przez poziomy
            for (int i = 7; i >= 0; i--)
            {
                //sortowanie poziomu tak by usuwać najrzadsze kolory
                Levels[i].Sort();

                for(int j = 0; j < Levels[i].Count; j++)
                {
                    if (ColorsCount <= colors_count)
                        return;

                    Node tmp = Levels[i][j];

                    ReductNode(tmp, step);
                }
            }
        }
    }
   

}
