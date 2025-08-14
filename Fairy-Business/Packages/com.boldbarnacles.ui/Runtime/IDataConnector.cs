


// used on DataConnector's that can receive data from UI-Elements (like e.g. "CustomButton" Component)
using UnityEngine;

public interface IDataConnector
{
    // implement this function to take on another IDataConnector that wants to mimic this ones in- and outputs
    public void AddDirectConnection(IDataConnector directConnectionTarget);
    public void RemoveDirectConnection(IDataConnector directConnectionTarget);
    public void TakeNewData(float newValue);
    public void SendNewData(float newValue);
}
// used on UI-elements that receive data from a DataConnector (like e.g. "ReflectorInput" Component)
public interface IDataReceiver
{
    public void TakeNewData(float newValue);
}