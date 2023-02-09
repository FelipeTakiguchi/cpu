using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

var count_line = 0;
List<(string, byte[])> tags = new List<(string, byte[])>{};

if (args.Length == 0)
{
    Console.WriteLine("Você precisa passar um parâmetro para o arquivo a ser montado.");
    return;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine("O arquivo especifiado não existe.");
    return;
}

StreamWriter writer = null;
StreamReader reader = null;

try
{
    writer = new StreamWriter("memory");
    writer.WriteLine("v2.0 raw");
    reader = new StreamReader(filePath);

    while (!reader.EndOfStream)
    {
        string line = reader.ReadLine();
        saveTags(line);
    }
    
    count_line = 0;
    reader.Close();
    writer.Close();
    writer = new StreamWriter("memory");
    writer.WriteLine("v2.0 raw");
    reader = new StreamReader(filePath);

    while (!reader.EndOfStream)
    {
        string line = reader.ReadLine();
        line = processLine(line);
        writer.Write(line);
        writer.Write(" ");
    }
}
catch (Exception ex)
{
    Console.WriteLine("O seguinte erro ocorreu durante o processo:");
    Console.WriteLine(ex.Message);
}
finally
{
    reader.Close();
    writer.Close();
}

void appendByte(ref byte[] opC, int init, byte v1, byte v2, byte v3, byte v4){
    opC[init] = v1;
    opC[init+1] = v2;
    opC[init+2] = v3;
    opC[init+3] = v4; 
}

byte[] fatoraByte(int value){
    byte[] result = new byte[4]{0, 0, 0 ,0};

    if(value >= 8){
        value-=8;
        result[0] = 1;    
    }
    if(value >= 4){
        value-=4;
        result[1] = 1;    
    }
    if(value >= 2){
        value-=2;
        result[2] = 1;    
    }
    if(value == 1){
        result[3] = 1;
    }

    return result;
}

byte[] fatoraByte2(int value){
    byte[] result = new byte[8]{0, 0, 0 ,0, 0, 0, 0, 0};

    if(value >= 128){
        value-=128;
        result[0] = 1;    
    }
    if(value >= 64){
        value-=64;
        result[1] = 1;    
    }
    if(value >= 32){
        value-=32;
        result[2] = 1; 
    }
    if(value >= 16){
        value-=16;
        result[3] = 1;    
    }
    if(value >= 8){
        value-=8;
        result[4] = 1;    
    }
    if(value >= 4){
        value-=4;
        result[5] = 1;    
    }
    if(value >= 2){
        value-=2;
        result[6] = 1;    
    }
    if(value == 1){
        result[7] = 1;
    }

    return result;
}

byte[] fatoraByte3(int value){
    byte[] result = new byte[12]{0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0};

    if(value >= 2048){
        value-=2048;
        result[0] = 1;    
    }
    if(value >= 1024){
        value-=1024;
        result[1] = 1;    
    }
    if(value >= 512){
        value-=512;
        result[2] = 1; 
    }
    if(value >= 256){
        value-=256;
        result[3] = 1;    
    }
    if(value >= 128){
        value-=128;
        result[4] = 1;    
    }
    if(value >= 64){
        value-=64;
        result[5] = 1;    
    }
    if(value >= 32){
        value-=32;
        result[6] = 1; 
    }
    if(value >= 16){
        value-=16;
        result[7] = 1;    
    }
    if(value >= 8){
        value-=8;
        result[8] = 1;    
    }
    if(value >= 4){
        value-=4;
        result[9] = 1;    
    }
    if(value >= 2){
        value-=2;
        result[10] = 1;    
    }
    if(value == 1){
        result[11] = 1;
    }

    return result;
}



void saveTags(string line)
{
    string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();

    if(parts[0][(parts[0].Length)-1] == ':'){
        parts[0] = parts[0].Split(":")[0];
        (string, byte[]) value = (parts[0], fatoraByte3(count_line));
        tags.Add(value);
        count_line--;
    }
    count_line++;
}

string processLine(string line)
{
    byte[] opCode = new byte[16];

    string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();

    //if gigantesco para as op
    string[] opList = new string[]{"add", "sub", "imul", "idiv", "shl", "shr", "neg", "and",
                                    "or", "not", "xor", "inc", "dec"};

    var count = 0;

    foreach(var value in opList){
        if(parts[0] == value){
            appendByte(ref opCode, 0, 0, 0, 0, 1);
            byte[] byts = fatoraByte(count);
            appendByte(ref opCode, 4, byts[0], byts[1], byts[2], byts[3]);
            
            if(parts[0] != "inc" && parts[0] != "dec"){
                if(parts[1][0] == '$' && parts[2][0] == '$'){
                    parts[1] = parts[1].Split("$")[1];
                    parts[2] = parts[2].Split("$")[1];
                }
            } else if(parts[1][0] == '$'){
                parts[1] = parts[1].Split("$")[1];
            }

            byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);
            
            if(parts[0] != "inc" && parts[0] != "dec"){
                byts = fatoraByte(Convert.ToInt32(parts[2]));
                appendByte(ref opCode, 12, byts[0], byts[1], byts[2], byts[3]);
            }
            else{
                appendByte(ref opCode, 12, 0, 0, 0, 0);    
            }
        }

        count++;
    }   

    if(parts[0] == "mov"){
        if(parts[1][0] == '[' && parts[1][(parts[1].Length)-1] == ']'){
            appendByte(ref opCode, 0, 0, 0, 1, 0);
            appendByte(ref opCode, 4, 0, 0, 1, 0);
            parts[1] = parts[1].Split("[")[1];
            parts[1] = parts[1].Split("]")[0];

            parts[1] = parts[1].Split("$")[1];
            parts[2] = parts[2].Split("$")[1];

            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);

            byts = fatoraByte(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 12, byts[0], byts[1], byts[2], byts[3]);
        } else if(parts[2][0] == '[' && parts[2][(parts[0].Length)-1] == ']'){
            appendByte(ref opCode, 0, 0, 0, 1, 0);
            appendByte(ref opCode, 4, 0, 0, 0, 1);

            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);

            parts[2] = parts[2].Split("[")[1];
            parts[2] = parts[2].Split("]")[0];
            
            byts = fatoraByte(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 12, byts[0], byts[1], byts[2], byts[3]);

        } else if(parts[1][0] == '$' && parts[2][0] == '$'){
            appendByte(ref opCode, 0, 0, 0, 1, 0);
            appendByte(ref opCode, 4, 0, 0, 0, 0);
            parts[1] = parts[1].Split("$")[1];
            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);
            parts[2] = parts[2].Split("$")[1];            
            byts = fatoraByte(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 12, byts[0], byts[1], byts[2], byts[3]);
        } else if(parts[1][0] == '$'){
            appendByte(ref opCode, 0, 0, 0, 1, 1);
            parts[1] = parts[1].Split("$")[1];
            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 4, byts[0], byts[1], byts[2], byts[3]);
            byte[] byts2 = fatoraByte2(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 8, byts2[0], byts2[1], byts2[2], byts2[3]);
            appendByte(ref opCode, 12, byts2[4], byts2[5], byts2[6], byts2[7]);
        }    
    }

    if(parts[0] == "cmp"){
        if(parts[1][0] == '$' && parts[2][0] == '$'){
            appendByte(ref opCode, 0, 0, 1, 0, 0);
            appendByte(ref opCode, 4, 0, 0, 0, 0);
            parts[1] = parts[1].Split("$")[1];
            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);
            parts[2] = parts[2].Split("$")[1];
            byts = fatoraByte(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 12, byts[0], byts[1], byts[2], byts[3]);
        }
        else if(parts[1][0] == '$' && parts[2][0] != '$'){
            appendByte(ref opCode, 0, 0, 1, 0, 0);
            parts[1] = parts[1].Split("$")[1];
            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 4, byts[0], byts[1], byts[2], byts[3]);
            byte[] byts2 = fatoraByte2(Convert.ToInt32(parts[2]));
            appendByte(ref opCode, 8, byts2[0], byts2[1], byts2[2], byts2[3]);
            appendByte(ref opCode, 12, byts2[4], byts2[5], byts2[6], byts2[7]);
        }
    }

    switch(parts[0]){
        case "push":
            appendByte(ref opCode, 0, 0, 0, 1, 0);
            appendByte(ref opCode, 4, 0, 0, 1, 1);
            byte[] byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);
            appendByte(ref opCode, 12, 0, 0, 0, 0);

            break;
        case "pop":
            appendByte(ref opCode, 0, 0, 0, 1, 0);
            appendByte(ref opCode, 4, 0, 1, 0, 0);
            byts = fatoraByte(Convert.ToInt32(parts[1]));
            appendByte(ref opCode, 8, byts[0], byts[1], byts[2], byts[3]);
            appendByte(ref opCode, 12, 0, 0, 0, 0);

            break;
        case "jump":
            appendByte(ref opCode, 0, 1, 0, 0, 0);
            byte[] valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            break;
        case "je":
            appendByte(ref opCode, 0, 1, 0, 0, 1);

            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "jne":
            appendByte(ref opCode, 0, 1, 0, 1, 0);
            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "jg":
            appendByte(ref opCode, 0, 1, 0, 1, 1);
            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "jge":
            appendByte(ref opCode, 0, 1, 1, 0, 0);
            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "jz":
            appendByte(ref opCode, 0, 1, 1, 0, 1);
            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "call":
            appendByte(ref opCode, 0, 1, 1, 1, 0);
            valorzao = null;

            foreach(var value in tags){
                if(value.Item1 == parts[1])
                {
                    valorzao = value.Item2;
                }
            }

            appendByte(ref opCode, 4, valorzao[0], valorzao[1], valorzao[2], valorzao[3]);
            appendByte(ref opCode, 8, valorzao[4], valorzao[5], valorzao[6], valorzao[7]);
            appendByte(ref opCode, 12, valorzao[8], valorzao[9], valorzao[10], valorzao[11]);
            
            break;
        case "ret":
            appendByte(ref opCode, 0, 1, 1, 1, 1);
            appendByte(ref opCode, 4, 1, 1, 1, 1);
            appendByte(ref opCode, 8, 1, 1, 1, 1);
            appendByte(ref opCode, 12, 1, 1, 1, 1);
            break;
    }

    if(parts[0][(parts[0].Length)-1] != ':'){
        count_line++;
        return toHex(opCode);
    }

    return "";
}

string toHex(byte[] code)
{
    string hex = null;
    string[] letras = new string[]{"A", "B", "C","'D", "E", "F"};

    for(int i = 0; i < code.Length/4; i++){
        int value = 0;

        value += code[i * 4] * 8;
        value += code[i * 4 + 1] * 4;
        value += code[i * 4 + 2] * 2;
        value += code[i * 4 + 3];

        if(value > 9)
            hex += letras[value-10];        
        else
            hex += (value.ToString());
    }

    return hex;
}