import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Observations } from './observations';

describe('Observations', () => {
  let component: Observations;
  let fixture: ComponentFixture<Observations>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Observations],
    }).compileComponents();

    fixture = TestBed.createComponent(Observations);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
