#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>
#import <CoreLocation/CoreLocation.h>


//YourViewController *rootController =(YourViewController*)[[(YourAppDelegate*)
//[[UIApplication sharedApplication]delegate] window] rootViewController];
#define getRootViewController [[[UIApplication sharedApplication].delegate window] rootViewController]

@interface PlayBand : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate,CLLocationManagerDelegate>
{
    //CBCentralManager *centralManager;
    //CBPeripheral *connectPeripheral;
    //CBCharacteristic *writeCharacteristic;
    NSString *bandData;
}

//@property NSString *bandData; <= read error
@property(nonatomic, retain) NSString *bandData;

//- (void)initialize;
//
- (int)Connect:(NSString*)deviceName Mode:(int)mode DataPerSecond:(int)rate;
- (int)InquireBatteryStatus;
- (int)ControlLED:(int)iParam1 Mode:(int)iParam2 Ratio:(int)iParam3 OnPeriod:(int)iParam4 OffPeriod:(int)iParam5;
- (int)DoVibrate:(int)iParam1 Strong:(int)iParam2 OnPeriod:(int)iParam3 OffPeriod:(int)iParam4;
- (int)Disconnect;
- (int)SetPowerMode:(int)iMode DataPerSecond:(int)iDataPerSecond DoSend:(bool)send;
- (int)SetMagneticMode:(int)mode;
- (int)Reconnect;

@end

/*
 extern "C" {
 
 int OnUnityConnect(char *iDeviceName, int iMode, int iDataPerSecond);
 
 int OnUnityReconnect();
 //
 char* OnUnityGetSensor () ;
 //
 int OnUnityDisconnect();
 
 int OnUnityChangeMode(int iMode, int iDataPerSecond);
 
 int OnUnityInquireBatteryStatus();
 
 int OnUnityControlLED(int iParam1, int iParam2, int iParam3, int iParam4,int iParam5);
 
 int OnUnityDoVibrate(int iParam1, int iParam2, int iParam3, int iParam4);
 //
 int OnUnityMagneticMode(int mode);
 //
 char* OnUnityGetCountry();
 }
 */