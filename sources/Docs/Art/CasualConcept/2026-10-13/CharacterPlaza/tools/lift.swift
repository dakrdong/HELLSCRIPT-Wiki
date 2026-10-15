// lift <in.png> <out.png> [x y]  - Vision foreground instance mask; keeps the input size.
// With a point (pixels, origin top-left) only the instance under it is kept, else all instances.
import Foundation
import Vision
import CoreImage
import ImageIO
import UniformTypeIdentifiers
let a = CommandLine.arguments
let inURL = URL(fileURLWithPath: a[1]), outURL = URL(fileURLWithPath: a[2])
let handler = VNImageRequestHandler(url: inURL)
let req = VNGenerateForegroundInstanceMaskRequest()
try handler.perform([req])
guard let obs = req.results?.first else { print("no instances"); exit(2) }
var chosen = obs.allInstances
let mask = obs.instanceMask
CVPixelBufferLockBaseAddress(mask, .readOnly)
let mw = CVPixelBufferGetWidth(mask), mh = CVPixelBufferGetHeight(mask)
let src = CGImageSourceCreateWithURL(inURL as CFURL, nil)!
let img = CGImageSourceCreateImageAtIndex(src, 0, nil)!
print("instances", obs.allInstances.count, "mask", mw, mh, "image", img.width, img.height)
if a.count >= 5, let x = Double(a[3]), let y = Double(a[4]) {
  let mx = Int(x / Double(img.width) * Double(mw)), my = Int(y / Double(img.height) * Double(mh))
  let rowBytes = CVPixelBufferGetBytesPerRow(mask)
  let base = CVPixelBufferGetBaseAddress(mask)!.assumingMemoryBound(to: UInt8.self)
  let label = Int(base[my * rowBytes + mx])
  print("label at point", label)
  if label == 0 { print("point is background"); exit(3) }
  chosen = IndexSet(integer: label)
}
CVPixelBufferUnlockBaseAddress(mask, .readOnly)
let buf = try obs.generateMaskedImage(ofInstances: chosen, from: handler, croppedToInstancesExtent: false)
let ci = CIImage(cvPixelBuffer: buf)
let ctx = CIContext()
let cg = ctx.createCGImage(ci, from: ci.extent)!
let dst = CGImageDestinationCreateWithURL(outURL as CFURL, UTType.png.identifier as CFString, 1, nil)!
CGImageDestinationAddImage(dst, cg, nil)
CGImageDestinationFinalize(dst)
print("wrote", cg.width, cg.height)
