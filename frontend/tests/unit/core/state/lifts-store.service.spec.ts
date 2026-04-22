import { LiftsStoreService } from '../../../../src/app/core/state/lifts-store.service';

describe('LiftsStoreService', () => {
  it('replaces current items when refreshing a different filtered view', () => {
    const service = new LiftsStoreService();

    service.setResponse({
      items: [
        { id: '1', name: 'Front Squat', isActive: true },
        { id: '2', name: 'Bench Press', isActive: true },
      ],
    });

    service.setResponse({
      items: [
        { id: '1', name: 'Front Squat', isActive: false },
        { id: '2', name: 'Bench Press', isActive: true },
        { id: '3', name: 'Deadlift', isActive: true },
      ],
    });

    expect(service.items()).toEqual([
      { id: '1', name: 'Front Squat', isActive: false },
      { id: '2', name: 'Bench Press', isActive: true },
      { id: '3', name: 'Deadlift', isActive: true },
    ]);
    expect(service.isLoaded()).toBeTrue();
  });

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

  it('upsert updates matching lift state without adding duplicates', () => {
    const service = new LiftsStoreService();

    service.setResponse({
      items: [{ id: '1', name: 'Front Squat', isActive: true }],
    });

    service.upsert({
      id: '1',
      name: 'Front Squat',
      isActive: false,
    });

    expect(service.items()).toEqual([{ id: '1', name: 'Front Squat', isActive: false }]);
  });
});
