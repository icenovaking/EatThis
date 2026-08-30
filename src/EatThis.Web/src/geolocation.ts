export type GeolocationPort = Pick<Geolocation, 'getCurrentPosition'>

export type LocationFailureKind =
  | 'permission-denied'
  | 'unsupported-geolocation'
  | 'unavailable'

export class LocationError extends Error {
  constructor(public readonly kind: LocationFailureKind, message: string) {
    super(message)
    this.name = 'LocationError'
  }
}

export function requestCurrentLocation(
  geolocation: GeolocationPort | undefined = globalThis.navigator?.geolocation,
): Promise<{ latitude: number; longitude: number }> {
  if (!geolocation || typeof geolocation.getCurrentPosition !== 'function') {
    return Promise.reject(
      new LocationError('unsupported-geolocation', '此瀏覽器不支援定位功能。'),
    )
  }

  return new Promise((resolve, reject) => {
    geolocation.getCurrentPosition(
      position => resolve({
        latitude: position.coords.latitude,
        longitude: position.coords.longitude,
      }),
      error => {
        const kind: LocationFailureKind = error.code === 1
          ? 'permission-denied'
          : 'unavailable'
        reject(new LocationError(kind, error.message || '目前無法取得位置。'))
      },
      {
        enableHighAccuracy: false,
        maximumAge: 60_000,
        timeout: 10_000,
      },
    )
  })
}
