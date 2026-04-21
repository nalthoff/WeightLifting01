import { LiftsStoreService } from '../../../../src/app/core/state/lifts-store.service';

describe('LiftsStoreService', () => {
  it('replaces an existing lift item and keeps sorted order', () => {
    const service = new LiftsStoreService();

    service.setResponse({
      items: [
        { id: '1', name: 'Front Squat', isActive: true },
        { id: '2', name: 'Overhead Press', isActive: true },
      ],
    });

    service.replace({
      id: '1',
      name: 'Paused Front Squat',
      isActive: true,
    });

    expect(service.items()).toEqual([
      { id: '2', name: 'Overhead Press', isActive: true },
      { id: '1', name: 'Paused Front Squat', isActive: true },
    ]);
  });

  it('does not mutate the list when replace target is missing', () => {
    const service = new LiftsStoreService();

    service.setResponse({
      items: [{ id: '1', name: 'Front Squat', isActive: true }],
    });

    service.replace({
      id: '99',
      name: 'Paused Front Squat',
      isActive: true,
    });

    expect(service.items()).toEqual([{ id: '1', name: 'Front Squat', isActive: true }]);
  });
});
