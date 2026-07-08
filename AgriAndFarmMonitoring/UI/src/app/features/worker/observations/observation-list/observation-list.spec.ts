import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ObservationList } from './observation-list';

describe('ObservationList', () => {
  let component: ObservationList;
  let fixture: ComponentFixture<ObservationList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ObservationList],
    }).compileComponents();

    fixture = TestBed.createComponent(ObservationList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
