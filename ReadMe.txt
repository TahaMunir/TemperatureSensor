MQTTPublisher: Publishes Temperature values to MQTTBroker at "localhost":1883 at topic "machine10/device06/temperature" 

MQTTSubscriber: Subscribes to the MQTTBroker at "localhost":1883 at topic "machine10/device06/temperature"
Persists the data in MongoDB with DBName= Measurements, CollectionName= SensorReadings and Document name will be the SensorID


WpfApp: In order to display charts the sensor Id and startTime should be provided
20 values starting from startTime will be displayed with Timestamps


TemperatureMeasurmeent
public string SensorId
public double Value 
public DateTime TimeStamp 