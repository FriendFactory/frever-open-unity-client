//
//  SwiftForUnity.swift
//  SwiftPlugin
//
//  Created by Zhuoyue Wang on 2020-05-18.
//  Copyright © 2020 Zhuoyue Wang. All rights reserved.
//
import Foundation
import UIKit
import CoreMedia
import CoreMotion
import SceneKit
import AVFoundation
import ARCore
import Accelerate

@available(iOS 13.0, *)
@objc public class SwiftForUnity: UIViewController, AVCaptureVideoDataOutputSampleBufferDelegate, SCNSceneRendererDelegate
{
    // Declare class that share with C#
    @objc public static let shared = SwiftForUnity()
    
    private let SKIP_SMOOTHING_THRESHOLD = Float(0.3)
    
    // Variable declaration
    private var faceSession : GARAugmentedFaceSession?
    private var captureDevice: AVCaptureDevice?
    private var captureSession: AVCaptureSession?
    private var videoFieldOfView = Float(0)
    private lazy var cameraImageLayer = CALayer()
    private lazy var motionManager = CMMotionManager()

    private var blendshapes = [Float](repeating: 0, count: 39);
    private var cachedBlendshapes = [Float](repeating: 0, count: 39);
    private var faceVerts = [Float](repeating: 0, count: 1404);
    private var faceEdges = [Float](repeating: 0, count: 1404);
    private var centerTransformArray4X4 = [Float](repeating: 0, count: 16);
    private var lastFrameCapturedVerts = false;
    private var lastRotation = UInt(0)


    // Setup a camera capture session from the front camera to receive captures.
    private func setupCamera() {
        
        if(self.captureSession?.isRunning == true)
        {
            return;
        }
        
        guard let device = AVCaptureDevice.default(.builtInWideAngleCamera, for: .video, position: .front),
        let input = try? AVCaptureDeviceInput(device: device)
            else {
                NSLog("Failed to create capture device from front camera.")
                return
            }

        let output = AVCaptureVideoDataOutput()
        output.videoSettings = [kCVPixelBufferPixelFormatTypeKey as String: kCVPixelFormatType_32BGRA]
        output.setSampleBufferDelegate(self, queue: DispatchQueue.global(qos: .userInteractive))

        let session = AVCaptureSession()
        session.beginConfiguration()
        session.sessionPreset = .high
        session.addInput(input)
        session.addOutput(output)
        session.commitConfiguration()
        captureSession = session
        captureDevice = device
        videoFieldOfView = captureDevice?.activeFormat.videoFieldOfView ?? 0

        cameraImageLayer.frame = self.view.bounds
        view.layer.insertSublayer(cameraImageLayer, at: 0)

        // Start capturing images from the capture session once permission is granted.
        getVideoPermission(permissionHandler: { granted in
            guard granted else {
                NSLog("Permission not granted to use camera.")
                return
            }

            self.captureSession?.startRunning()
        })
    }

    // Start receiving motion updates to determine device orientation for use in the face session.
    private func setupMotion() {
        guard motionManager.isDeviceMotionAvailable else {
            NSLog("Device does not have motion sensors.")
            return
        }
        motionManager.deviceMotionUpdateInterval = 0.01
        motionManager.startDeviceMotionUpdates()
    }

    // Get permission to use device camera.
    // - Parameters:
    //  - permissionHandler: The closure to call with whether permission was granted when permission is determined.
    private func getVideoPermission(permissionHandler: @escaping (Bool) -> ()) {
        switch AVCaptureDevice.authorizationStatus(for: .video) {
        case .authorized:
            permissionHandler(true)
        case .notDetermined:
            AVCaptureDevice.requestAccess(for: .video, completionHandler: permissionHandler)
        default:
            permissionHandler(false)
        }
    }

    // Only for connection testing
    @objc public func SayHelloToUnity() -> String{
        return "Hello, this is Swift"
    }
    
    // Initialize camera, motion and ARCore face session
    @objc override public func viewDidLoad() {
        super.viewDidLoad()
        setupCamera()
        setupMotion()
        faceSession = try! GARAugmentedFaceSession(fieldOfView: videoFieldOfView)
      }

    // AVCaptureVideoDataOutputSampleBufferDelegate
    public func captureOutput(
        _ output: AVCaptureOutput,
        didOutput sampleBuffer: CMSampleBuffer,
        from connection: AVCaptureConnection
        ) {
            guard let imgBuffer = CMSampleBufferGetImageBuffer(sampleBuffer),
            let deviceMotion = motionManager.deviceMotion
            else { return }

            let frameTime = CMTimeGetSeconds(CMSampleBufferGetPresentationTimeStamp(sampleBuffer))

            // Use the device's gravity vector to determine which direction is up for a face. This is the
            // positive counter-clockwise rotation of the device relative to landscape left orientation.
            let rotation =  2 * .pi - atan2(deviceMotion.gravity.x, deviceMotion.gravity.y) + .pi / 2
            let rotationDegrees = (UInt)(rotation * 180 / .pi) % 360

            lastRotation = rotationDegrees; // will cleanup later but helps to lock this so less moving parts
            faceSession?.update(with: imgBuffer, timestamp: frameTime, recognitionRotation: rotationDegrees)
    }
    
    @objc public func getFaceVertices() -> [Float]
    {
       lastFrameCapturedVerts = false
       guard let frame = faceSession?.currentFrame else {return faceVerts}
       if let face = frame.face
       {
            let vtxCount = faceVerts.count / 3;
            for i in 0..<vtxCount
            {
                faceVerts[i*3] = face.mesh.vertices[i][0];
                faceVerts[i*3+1] = face.mesh.vertices[i][1];
                faceVerts[i*3+2] = face.mesh.vertices[i][2];
            }
            lastFrameCapturedVerts = true;
       }
       return faceVerts;
    }
    
    @objc public func getWhetherLastFrameCapturedVerts() -> Bool
    {
        return lastFrameCapturedVerts;
    }
    
     @objc public func getLastRotation() -> UInt
     {
         return lastRotation;
     }
    
    
    @objc public func getCenterTransform() -> [Float]
    {
       /*  - (simd_float4x4) centerTransform
              The transform from camera to the center of the face, defined to have the origin located behind the nose and between the two cheek bones.
              +Z is forward out of the nose, +Y is upwards, and +X is towards the face's left. The units are in meters. */
        
       guard let frame = faceSession?.currentFrame else {return centerTransformArray4X4}
       if let face = frame.face
       {
            let vtxCount = centerTransformArray4X4.count / 4;
            for i in 0..<vtxCount
            {
                centerTransformArray4X4[i*4] = face.centerTransform[i][0];
                centerTransformArray4X4[i*4+1] = face.centerTransform[i][1];
                centerTransformArray4X4[i*4+2] = face.centerTransform[i][2];
                centerTransformArray4X4[i*4+3] = face.centerTransform[i][3];
            }
       }
       return centerTransformArray4X4;
    }
        
    
 }
