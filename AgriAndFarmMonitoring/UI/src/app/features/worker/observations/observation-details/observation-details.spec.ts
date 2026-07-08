import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ObservationDetails } from './observation-details';

describe('ObservationDetails', () => {
  let component: ObservationDetails;
  let fixture: ComponentFixture<ObservationDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ObservationDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(ObservationDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
